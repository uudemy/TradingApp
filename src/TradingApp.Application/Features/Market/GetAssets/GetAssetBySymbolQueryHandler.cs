using MediatR;
using TradingApp.Application.Abstractions;
using TradingApp.Application.Features.Market.Dtos;
using TradingApp.Application.Features.Market.GetAssets;
using TradingApp.Domain.Common;

namespace TradingApp.Application.Features.Market.GetAssetBySymbol;

public sealed class GetAssetBySymbolQueryHandler
    : IRequestHandler<GetAssetBySymbolQuery, AssetDto>
{
    private readonly ISender _sender;

    public GetAssetBySymbolQueryHandler(ISender sender) => _sender = sender;

    public async Task<AssetDto> Handle(GetAssetBySymbolQuery request, CancellationToken ct)
    {
        var list = await _sender.Send(new GetAssetsQuery(), ct);
        var symbol = request.Symbol.Trim().ToUpperInvariant();

        return list.FirstOrDefault(a => a.Symbol == symbol)
            ?? throw new DomainException("asset_not_found", $"Asset '{symbol}' not found.");
    }
}