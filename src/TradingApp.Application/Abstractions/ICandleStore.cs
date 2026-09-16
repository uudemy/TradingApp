using TradingApp.Application.Features.Market.Dtos;

namespace TradingApp.Application.Abstractions;

public interface ICandleStore
{
    Task<IReadOnlyCollection<CandleDto>> GetCandlesAsync(
        string symbol, string interval, int limit, CancellationToken ct = default);

    Task AppendTickAsync(string symbol, decimal price, decimal volume, CancellationToken ct = default);
}