using MediatR;
using TradingApp.Application.Abstractions;
using TradingApp.Application.Features.Market.Dtos;

namespace TradingApp.Application.Features.Market.GetMarketPrice;

public sealed class GetMarketPriceQueryHandler
    : IRequestHandler<GetMarketPriceQuery, MarketPriceDto>
{
    private readonly IMarketDataProvider _provider;
    public GetMarketPriceQueryHandler(IMarketDataProvider provider) => _provider = provider;

    public Task<MarketPriceDto> Handle(GetMarketPriceQuery request, CancellationToken ct)
        => _provider.GetPriceAsync(request.Symbol, ct);
}