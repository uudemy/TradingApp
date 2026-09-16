using System.Globalization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TradingApp.Infrastructure.MarketData.Options;

namespace TradingApp.Infrastructure.MarketData.Stooq;

public sealed class StooqClient
{
    private readonly HttpClient _http;
    private readonly StooqOptions _opt;
    private readonly ILogger<StooqClient> _logger;

    public StooqClient(
        HttpClient http,
        IOptions<MarketDataOptions> options,
        ILogger<StooqClient> logger)
    {
        _http = http;
        _logger = logger;
        _opt = options.Value.Stooq;

        _http.BaseAddress = new Uri(_opt.BaseUrl);
        _http.Timeout = TimeSpan.FromSeconds(_opt.RequestTimeoutSeconds);
    }

    /// <summary>Anlık fiyat (CSV).</summary>
    public async Task<StooqQuote?> GetQuoteAsync(string stooqSymbol, CancellationToken ct)
    {
        var url = $"/q/l/?s={Uri.EscapeDataString(stooqSymbol)}&f=sd2t2ohlcv&h&e=csv";
        var csv = await _http.GetStringAsync(url, ct);

        var lines = csv.Split('\n', StringSplitOptions.RemoveEmptyEntries);
        if (lines.Length < 2) return null;

        var values = lines[1].Split(',');
        if (values.Length < 8) return null;

        // CSV: Symbol,Date,Time,Open,High,Low,Close,Volume
        if (!decimal.TryParse(values[3], NumberStyles.Any, CultureInfo.InvariantCulture, out var open)) return null;
        if (!decimal.TryParse(values[4], NumberStyles.Any, CultureInfo.InvariantCulture, out var high)) return null;
        if (!decimal.TryParse(values[5], NumberStyles.Any, CultureInfo.InvariantCulture, out var low)) return null;
        if (!decimal.TryParse(values[6], NumberStyles.Any, CultureInfo.InvariantCulture, out var close)) return null;
        long.TryParse(values[7], NumberStyles.Any, CultureInfo.InvariantCulture, out var volume);

        var date = values[1];
        var time = values[2];
        DateTimeOffset.TryParse($"{date} {time}", CultureInfo.InvariantCulture,
            DateTimeStyles.AssumeUniversal, out var timestamp);

        return new StooqQuote
        {
            Symbol = values[0],
            Open = open,
            High = high,
            Low = low,
            Close = close,
            Volume = volume,
            Timestamp = timestamp
        };
    }

    /// <summary>Günlük OHLCV geçmişi (CSV).</summary>
    public async Task<List<StooqBar>> GetDailyHistoryAsync(string stooqSymbol, CancellationToken ct)
    {
        var url = $"/q/d/l/?s={Uri.EscapeDataString(stooqSymbol)}&i=d";
        var csv = await _http.GetStringAsync(url, ct);

        var lines = csv.Split('\n', StringSplitOptions.RemoveEmptyEntries);
        if (lines.Length < 2) return new List<StooqBar>();

        var bars = new List<StooqBar>(lines.Length - 1);
        for (int i = 1; i < lines.Length; i++)
        {
            var values = lines[i].Split(',');
            if (values.Length < 6) continue;

            // CSV: Date,Open,High,Low,Close,Volume
            if (!DateTimeOffset.TryParse(values[0], CultureInfo.InvariantCulture,
                    DateTimeStyles.AssumeUniversal, out var dt)) continue;
            if (!decimal.TryParse(values[1], NumberStyles.Any, CultureInfo.InvariantCulture, out var open)) continue;
            if (!decimal.TryParse(values[2], NumberStyles.Any, CultureInfo.InvariantCulture, out var high)) continue;
            if (!decimal.TryParse(values[3], NumberStyles.Any, CultureInfo.InvariantCulture, out var low)) continue;
            if (!decimal.TryParse(values[4], NumberStyles.Any, CultureInfo.InvariantCulture, out var close)) continue;
            long.TryParse(values[5], NumberStyles.Any, CultureInfo.InvariantCulture, out var volume);

            bars.Add(new StooqBar
            {
                Date = dt,
                Open = open,
                High = high,
                Low = low,
                Close = close,
                Volume = volume
            });
        }

        return bars;
    }
}

public sealed class StooqQuote
{
    public string? Symbol { get; set; }
    public decimal Open { get; set; }
    public decimal High { get; set; }
    public decimal Low { get; set; }
    public decimal Close { get; set; }
    public long Volume { get; set; }
    public DateTimeOffset Timestamp { get; set; }
}

public sealed class StooqBar
{
    public DateTimeOffset Date { get; set; }
    public decimal Open { get; set; }
    public decimal High { get; set; }
    public decimal Low { get; set; }
    public decimal Close { get; set; }
    public long Volume { get; set; }
}