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

        // --- Cash balances ---
        var wallets = await _db.Wallets.AsNoTracking()
            .Where(w => w.UserId == userId)
            .OrderBy(w => w.Currency)
            .ToListAsync(ct);

        var cash = wallets.Select(w => new CashBalanceDto(
            w.Currency,
            w.AvailableBalance,
            w.LockedBalance,
            w.TotalBalance)).ToArray();

        // --- Positions ---
        var positions = await _db.PortfolioPositions.AsNoTracking()
            .Where(p => p.PortfolioId == portfolio.Id && p.Quantity > 0m)
            .ToListAsync(ct);

        if (positions.Count == 0)
        {
            return new PortfolioSummaryDto(
                0m, 0m, 0m, 0m, 0m, cash, Array.Empty<PositionDto>());
        }

        var assetIds = positions.Select(p => p.AssetId).ToList();
        var assets = await _db.Assets.AsNoTracking()
            .Where(a => assetIds.Contains(a.Id))
            .ToDictionaryAsync(a => a.Id, ct);

        var dtos = new List<PositionDto>(positions.Count);
        decimal totalValue = 0m, totalCost = 0m, totalDailyPnl = 0m;

        foreach (var p in positions)
        {
            if (!assets.TryGetValue(p.AssetId, out var asset)) continue;

            var marketValue = p.MarketValue(asset.CurrentPrice);
            var costBasis = p.Quantity * p.AverageCost;
            var unrealized = p.UnrealizedPnl(asset.CurrentPrice);
            var unrealizedPct = costBasis == 0m ? 0m : Math.Round(unrealized / costBasis * 100m, 4);
            var dailyPnl = p.DailyPnl(asset.CurrentPrice, asset.PreviousClose);

            totalValue += marketValue;
            totalCost += costBasis;
            totalDailyPnl += dailyPnl;

            dtos.Add(new PositionDto(
                p.Id,
                p.AssetId,
                asset.Symbol,
                asset.Name,
                asset.Currency,
                p.Quantity,
                p.LockedQuantity,
                p.AvailableQuantity,
                p.AverageCost,
                asset.CurrentPrice,
                asset.PreviousClose,
                marketValue,
                costBasis,
                unrealized,
                unrealizedPct,
                dailyPnl,
                p.RealizedPnl,
                p.PurchaseDate,
                p.Notes));
        }

        var totalUnrealized = totalValue - totalCost;
        var totalUnrealizedPct = totalCost == 0m ? 0m
            : Math.Round(totalUnrealized / totalCost * 100m, 4);

        return new PortfolioSummaryDto(
            totalValue,
            totalCost,
            totalUnrealized,
            totalUnrealizedPct,
            totalDailyPnl,
            cash,
            dtos.OrderByDescending(d => d.MarketValue).ToArray());
    }
}