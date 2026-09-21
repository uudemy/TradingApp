using TradingApp.Application.Features.Market.Dtos;

namespace TradingApp.Application.Abstractions;

public interface IMarketDataCache
{
    Task<IReadOnlyCollection<AssetDto>?> GetAssetListAsync(CancellationToken ct = default);
    Task SetAssetListAsync(IReadOnlyCollection<AssetDto> assets, TimeSpan ttl, CancellationToken ct = default);

    Task<MarketPriceDto?> GetPriceAsync(string symbol, CancellationToken ct = default);
    Task SetPriceAsync(MarketPriceDto price, TimeSpan ttl, CancellationToken ct = default);

    // Candle cache
    Task<IReadOnlyCollection<CandleDto>?> GetCandlesAsync(
        string symbol, string interval, CancellationToken ct = default);

    Task SetCandlesAsync(
        string symbol, string interval,
        IReadOnlyCollection<CandleDto> candles,
        TimeSpan ttl,
        CancellationToken ct = default);

    Task InvalidateAssetListAsync(CancellationToken ct = default);
}