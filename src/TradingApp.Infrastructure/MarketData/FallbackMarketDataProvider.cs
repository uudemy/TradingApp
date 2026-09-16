using Microsoft.Extensions.Logging;
using TradingApp.Application.Abstractions;
using TradingApp.Application.Features.Market.Dtos;

namespace TradingApp.Infrastructure.MarketData;

/// <summary>
/// Önce Yahoo'yu dener; hata alırsa Mock'a düşer.
/// Prod'da fallback kapatılır, dev'de açık kalır.
/// </summary>
public sealed class FallbackMarketDataProvider : IMarketDataProvider
{
    private readonly IMarketDataProvider _primary;
    private readonly IMarketDataProvider _fallback;
    private readonly ILogger<FallbackMarketDataProvider> _logger;

    public FallbackMarketDataProvider(
        IMarketDataProvider primary,
        IMarketDataProvider fallback,
        ILogger<FallbackMarketDataProvider> logger)
    {
        _primary = primary;
        _fallback = fallback;
        _logger = logger;
    }

    public async Task<MarketPriceDto> GetPriceAsync(string symbol, CancellationToken ct = default)
    {
        try { return await _primary.GetPriceAsync(symbol, ct); }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Primary provider failed for {Symbol}, falling back to mock.", symbol);
            return await _fallback.GetPriceAsync(symbol, ct);
        }
    }

    public async Task<IReadOnlyCollection<MarketPriceDto>> GetPricesAsync(CancellationToken ct = default)
    {
        try { return await _primary.GetPricesAsync(ct); }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Primary provider failed, falling back to mock.");
            return await _fallback.GetPricesAsync(ct);
        }
    }

    public async Task<IReadOnlyCollection<CandleDto>> GetCandlesAsync(
        string symbol, string interval, int limit, CancellationToken ct = default)
    {
        try { return await _primary.GetCandlesAsync(symbol, interval, limit, ct); }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Primary candles failed for {Symbol}/{Interval}, falling back.", symbol, interval);
            return await _fallback.GetCandlesAsync(symbol, interval, limit, ct);
        }
    }
}