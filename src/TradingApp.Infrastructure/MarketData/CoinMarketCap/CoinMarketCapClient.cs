using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TradingApp.Infrastructure.MarketData.Options;

namespace TradingApp.Infrastructure.MarketData.CoinMarketCap;

public sealed class CoinMarketCapClient
{
    private readonly HttpClient _http;
    private readonly ILogger<CoinMarketCapClient> _logger;

    private static readonly JsonSerializerOptions JsonOpts = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public CoinMarketCapClient(
        HttpClient http,
        IOptions<MarketDataOptions> options,
        ILogger<CoinMarketCapClient> logger)
    {
        _http = http;
        _logger = logger;

        var opt = options.Value.CoinMarketCap;
        _http.BaseAddress = new Uri(opt.BaseUrl);
        _http.Timeout = TimeSpan.FromSeconds(opt.RequestTimeoutSeconds);
    }

    /// <summary>CoinMarketCap ID'leri ile anlık fiyat çeker (keyless).</summary>
    public async Task<CmcSimplePriceResponse> GetSimplePriceAsync(
        IEnumerable<string> cmcIds, CancellationToken ct)
    {
        var ids = string.Join(",", cmcIds);
        var url = $"/v1/simple/price?ids={ids}&convert=USD";

        using var resp = await _http.GetAsync(url, ct);
        if (!resp.IsSuccessStatusCode)
        {
            _logger.LogWarning("CMC /simple/price HTTP {Status}", (int)resp.StatusCode);
            resp.EnsureSuccessStatusCode();
        }

        await using var stream = await resp.Content.ReadAsStreamAsync(ct);
        var payload = await JsonSerializer.DeserializeAsync<CmcSimplePriceResponse>(stream, JsonOpts, ct)
            ?? throw new InvalidOperationException("CMC response empty.");

        if (payload.Status?.ErrorCode is > 0)
            throw new InvalidOperationException($"CMC error: {payload.Status.ErrorCode} {payload.Status.ErrorMessage}");

        return payload;
    }
}