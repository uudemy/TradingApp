using MediatR;
using TradingApp.Application.Features.Market.Dtos;

namespace TradingApp.Application.Features.Market.GetAssetBySymbol;

public sealed record GetAssetBySymbolQuery(string Symbol) : IRequest<AssetDto>;