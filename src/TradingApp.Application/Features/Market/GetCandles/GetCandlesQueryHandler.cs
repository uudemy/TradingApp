using MediatR;
using TradingApp.Application.Abstractions;
using TradingApp.Application.Features.Market.Dtos;

namespace TradingApp.Application.Features.Market.GetCandles;

public sealed class GetCandlesQueryHandler
    : IRequestHandler<GetCandlesQuery, IReadOnlyCollection<CandleDto>>
{
    private readonly IMarketDataProvider _provider;

    public GetCandlesQueryHandler(IMarketDataProvider provider) => _provider = provider;

    public Task<IReadOnlyCollection<CandleDto>> Handle(
        GetCandlesQuery request, CancellationToken ct)
        => _provider.GetCandlesAsync(request.Symbol, request.Interval, request.Limit, ct);
}