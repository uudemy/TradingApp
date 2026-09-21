using System.Globalization;
using System.Net.Http.Headers;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TradingApp.Application.Features.Market.Dtos;
using TradingApp.Infrastructure.MarketData.Options;

namespace TradingApp.Infrastructure.MarketData.Yahoo;

/// <summary>
/// Yahoo Finance HTTP istemcisi. Ham veri çeker, DTO'ya çevirir.
/// Hata durumunda exception fırlatır; fallback üst katmanda.
/// </summary>
public sealed class YahooFinanceClient
{
    private readonly HttpClient _http;
    private readonly ILogger<YahooFinanceClient> _logger;

    private static readonly JsonSerializerOptions JsonOpts = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public YahooFinanceClient(HttpClient http, IOptions<MarketDataOptions> options, ILogger<YahooFinanceClient> logger)
    {
        _http = http;
        _logger = logger;

        var opt = options.Value.Yahoo;
        _http.BaseAddress = new Uri(opt.BaseUrl);
        _http.Timeout = TimeSpan.FromSeconds(opt.RequestTimeoutSeconds);
        _http.DefaultRequestHeaders.UserAgent.ParseAdd(opt.UserAgent);
        _http.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
    }

    /// <summary>Anlık fiyat — 1 günlük chart verisinden meta okur.</summary>
    public async Task<YahooChartResult> GetQuoteAsync(string yahooSymbol, CancellationToken ct)
    {
        var url = $"/v8/finance/chart/{Uri.EscapeDataString(yahooSymbol)}?interval=1m&range=1d";
        var result = await FetchChartAsync(url, ct);

        if (result?.Meta?.RegularMarketPrice is null)
            throw new InvalidOperationException($"Yahoo quote failed for {yahooSymbol}.");

        return result;
    }

    /// <summary>OHLC mum verisi.</summary>
    public async Task<YahooChartResult> GetChartAsync(string yahooSymbol, string interval, CancellationToken ct)
    {
        var (yahooInterval, range) = MapInterval(interval);
        var url = $"/v8/finance/chart/{Uri.EscapeDataString(yahooSymbol)}?interval={yahooInterval}&range={range}";
        var result = await FetchChartAsync(url, ct);

        if (result?.Timestamps is null || result.Indicators?.Quote is null)
            throw new InvalidOperationException($"Yahoo candles failed for {yahooSymbol}.");

        return result;
    }

    private async Task<YahooChartResult?> FetchChartAsync(string url, CancellationToken ct)
    {
        using var resp = await _http.GetAsync(url, ct);

        if (!resp.IsSuccessStatusCode)
        {
            _logger.LogWarning("Yahoo HTTP {Status} for {Url}", (int)resp.StatusCode, url);
            resp.EnsureSuccessStatusCode();
        }

        await using var stream = await resp.Content.ReadAsStreamAsync(ct);
        var payload = await JsonSerializer.DeserializeAsync<YahooChartResponse>(stream, JsonOpts, ct);

        if (payload?.Chart?.Error is { } err && !string.IsNullOrEmpty(err.Code))
            throw new InvalidOperationException($"Yahoo error: {err.Code} {err.Description}");

        return payload?.Chart?.Result?.FirstOrDefault();
    }

    /// <summary>Bizim interval'imizi Yahoo'nun (interval, range) çiftine çevirir.</summary>
    private static (string interval, string range) MapInterval(string interval) => interval switch
    {
        "1s"  => ("1m", "1d"),
        "1m"  => ("1m", "1d"),
        "5m"  => ("5m", "5d"),
        "15m" => ("15m", "5d"),
        "1h"  => ("60m", "1mo"),
        "4h"  => ("60m", "3mo"),
        "1D"  => ("1d", "2y"),
        "1W"  => ("1wk", "10y"),
        "1MO" => ("1mo", "10y"),    // 1 ay
        "1M"  => ("1mo", "10y"),    // legacy fallback
        _     => ("1d", "2y")
    };

    public static DateTimeOffset FromUnix(long unixSeconds)
        => DateTimeOffset.FromUnixTimeSeconds(unixSeconds);

    public static decimal ToDecimal(long value) => value;
}