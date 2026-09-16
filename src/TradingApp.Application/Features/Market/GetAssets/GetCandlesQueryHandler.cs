using MediatR;
using TradingApp.Application.Abstractions;
using TradingApp.Application.Features.Market.Dtos;

namespace TradingApp.Application.Features.Market.GetCandles;

public sealed class GetCandlesQueryHandler
    : IRequestHandler<GetCandlesQuery, IReadOnlyCollection<CandleDto>>
{
    private readonly ICandleStore _store;
    public GetCandlesQueryHandler(ICandleStore store) => _store = store;

    public Task<IReadOnlyCollection<CandleDto>> Handle(GetCandlesQuery request, CancellationToken ct)
        => _store.GetCandlesAsync(request.Symbol, request.Interval, request.Limit, ct);
}