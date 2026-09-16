using MediatR;
using TradingApp.Application.Features.Market.Dtos;

namespace TradingApp.Application.Features.Market.GetAssets;

public sealed record GetAssetsQuery : IRequest<IReadOnlyCollection<AssetDto>>;