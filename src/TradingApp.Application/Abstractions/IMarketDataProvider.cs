using TradingApp.Application.Features.Market.Dtos;

namespace TradingApp.Application.Abstractions;

public interface IMarketDataProvider
{
    Task<MarketPriceDto> GetPriceAsync(string symbol, CancellationToken ct = default);
    Task<IReadOnlyCollection<MarketPriceDto>> GetPricesAsync(CancellationToken ct = default);

    Task<IReadOnlyCollection<CandleDto>> GetCandlesAsync(
        string symbol, string interval, int limit, CancellationToken ct = default);
}