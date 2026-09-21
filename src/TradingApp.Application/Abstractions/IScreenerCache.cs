using TradingApp.Application.Features.Analysis.Dtos;

namespace TradingApp.Application.Abstractions;

public interface IScreenerCache
{
    Task<ScreenerResultDto?> GetAsync(string key, CancellationToken ct = default);
    Task SetAsync(string key, ScreenerResultDto result, TimeSpan ttl, CancellationToken ct = default);
}