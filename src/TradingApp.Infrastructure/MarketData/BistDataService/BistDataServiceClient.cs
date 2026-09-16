using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TradingApp.Infrastructure.MarketData.Options;

namespace TradingApp.Infrastructure.MarketData.BistDataService;

public sealed class BistDataServiceClient
{
    private readonly HttpClient _http;
    private readonly BistDataServiceOptions _opt;
    private readonly ILogger<BistDataServiceClient> _logger;

    private static readonly JsonSerializerOptions JsonOpts = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public BistDataServiceClient(
        HttpClient http,
        IOptions<MarketDataOptions> options,
        ILogger<BistDataServiceClient> logger)
    {
        _http = http;
        _logger = logger;
        _opt = options.Value.BistDataService;

        _http.BaseAddress = new Uri(_opt.BaseUrl);
        _http.Timeout = TimeSpan.FromSeconds(_opt.RequestTimeoutSeconds);

        if (_opt.AuthRequired && !string.IsNullOrEmpty(_opt.ApiKey))
            _http.DefaultRequestHeaders.Add("X-API-Key", _opt.ApiKey);
    }

    /// <summary>Tüm BIST hisselerinin anlık fiyatları.</summary>
    public async Task<List<BistQuote>> GetAllQuotesAsync(CancellationToken ct)
    {
        using var resp = await _http.GetAsync("/all", ct);
        if (!resp.IsSuccessStatusCode)
        {
            _logger.LogWarning("BistDataService /all HTTP {Status}", (int)resp.StatusCode);
            resp.EnsureSuccessStatusCode();
        }

        await using var stream = await resp.Content.ReadAsStreamAsync(ct);
        var payload = await JsonSerializer.DeserializeAsync<BistAllResponse>(stream, JsonOpts, ct);
        return payload?.Data ?? new List<BistQuote>();
    }

    /// <summary>Tek hisse için anlık fiyat.</summary>
    public async Task<BistQuote?> GetQuoteAsync(string symbol, CancellationToken ct)
    {
        var all = await GetAllQuotesAsync(ct);
        return all.FirstOrDefault(q =>
            string.Equals(q.Symbol, symbol, StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>OHLCV geçmiş verisi.</summary>
    public async Task<List<BistBar>> GetHistoryAsync(string symbol, CancellationToken ct)
    {
        using var resp = await _http.GetAsync($"/history/{Uri.EscapeDataString(symbol)}", ct);
        if (!resp.IsSuccessStatusCode)
        {
            _logger.LogWarning("BistDataService /history/{Symbol} HTTP {Status}", symbol, (int)resp.StatusCode);
            resp.EnsureSuccessStatusCode();
        }

        await using var stream = await resp.Content.ReadAsStreamAsync(ct);
        var payload = await JsonSerializer.DeserializeAsync<BistHistoryResponse>(stream, JsonOpts, ct);
        return payload?.Data ?? new List<BistBar>();
    }
}