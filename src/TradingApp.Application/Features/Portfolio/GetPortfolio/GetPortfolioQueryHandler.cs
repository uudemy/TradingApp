using MediatR;
using Microsoft.EntityFrameworkCore;
using TradingApp.Application.Abstractions;
using TradingApp.Application.Features.Portfolio.Dtos;
using TradingApp.Domain.Common;

namespace TradingApp.Application.Features.Portfolio.GetPortfolio;

public sealed class GetPortfolioQueryHandler
    : IRequestHandler<GetPortfolioQuery, PortfolioSummaryDto>
{
    private readonly IAppDbContext _db;
    private readonly ICurrentUserService _currentUser;

    public GetPortfolioQueryHandler(IAppDbContext db, ICurrentUserService currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task<PortfolioSummaryDto> Handle(
        GetPortfolioQuery request, CancellationToken ct)
    {
        var userId = _currentUser.UserId
            ?? throw new DomainException("unauthorized", "Authentication required.");

        var portfolio = await _db.Portfolios
            .FirstOrDefaultAsync(p => p.UserId == userId, ct)
            ?? throw new DomainException("portfolio_missing", "Portfolio not found.");

        var positions = await _db.PortfolioPositions.AsNoTracking()
            .Where(p => p.PortfolioId == portfolio.Id && p.Quantity > 0m)
            .ToListAsync(ct);

        if (positions.Count == 0)
            return new PortfolioSummaryDto(0m, 0m, 0m, 0m, Array.Empty<PositionDto>());

        var assetIds = positions.Select(p => p.AssetId).ToList();
        var assets = await _db.Assets.AsNoTracking()
            .Where(a => assetIds.Contains(a.Id))
            .ToDictionaryAsync(a => a.Id, ct);

        var dtos = new List<PositionDto>(positions.Count);
        decimal totalValue = 0m, totalCost = 0m;

        foreach (var p in positions)
        {
            if (!assets.TryGetValue(p.AssetId, out var asset)) continue;

            var marketValue = p.MarketValue(asset.CurrentPrice);
            var costBasis = p.Quantity * p.AverageCost;
            var unrealized = p.UnrealizedPnl(asset.CurrentPrice);
            var unrealizedPct = costBasis == 0m ? 0m : Math.Round(unrealized / costBasis * 100m, 4);

            totalValue += marketValue;
            totalCost += costBasis;

            dtos.Add(new PositionDto(
                p.AssetId,
                asset.Symbol,
                asset.Name,
                p.Quantity,
                p.LockedQuantity,
                p.AvailableQuantity,
                p.AverageCost,
                asset.CurrentPrice,
                marketValue,
                unrealized,
                unrealizedPct,
                p.RealizedPnl));
        }

        var totalUnrealized = totalValue - totalCost;
        var totalUnrealizedPct = totalCost == 0m ? 0m
            : Math.Round(totalUnrealized / totalCost * 100m, 4);

        return new PortfolioSummaryDto(
            totalValue,
            totalCost,
            totalUnrealized,
            totalUnrealizedPct,
            dtos.OrderByDescending(d => d.MarketValue).ToArray());
    }
}