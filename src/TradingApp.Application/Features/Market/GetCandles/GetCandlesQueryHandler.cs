using MediatR;
using TradingApp.Application.Abstractions;
using TradingApp.Application.Common;
using TradingApp.Application.Features.Market.Dtos;

namespace TradingApp.Application.Features.Market.GetCandles;

public sealed class GetCandlesQueryHandler
    : IRequestHandler<GetCandlesQuery, IReadOnlyCollection<CandleDto>>
{
    private readonly IMarketDataProvider _provider;
    private readonly IMarketDataCache _cache;

    public GetCandlesQueryHandler(IMarketDataProvider provider, IMarketDataCache cache)
    {
        _provider = provider;
        _cache = cache;
    }

    public async Task<IReadOnlyCollection<CandleDto>> Handle(
        GetCandlesQuery request, CancellationToken ct)
    {
        var symbol = request.Symbol.Trim().ToUpperInvariant();
        var interval = request.Interval;

        var cached = await _cache.GetCandlesAsync(symbol, interval, ct);
        if (cached is not null && cached.Count > 0)
            return cached;

        var candles = await _provider.GetCandlesAsync(symbol, interval, request.Limit, ct);

        if (candles.Count > 0)
        {
            var ttl = CacheTtl.ForInterval(interval);
            await _cache.SetCandlesAsync(symbol, interval, candles, ttl, ct);
        }

        return candles;
    }
}