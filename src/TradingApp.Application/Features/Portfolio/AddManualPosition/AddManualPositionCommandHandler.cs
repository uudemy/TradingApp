using MediatR;
using Microsoft.EntityFrameworkCore;
using TradingApp.Application.Abstractions;
using TradingApp.Domain.Common;
using TradingApp.Domain.Entities;

namespace TradingApp.Application.Features.Portfolio.AddManualPosition;

public sealed class AddManualPositionCommandHandler
    : IRequestHandler<AddManualPositionCommand, Guid>
{
    private readonly IAppDbContext _db;
    private readonly ICurrentUserService _currentUser;

    public AddManualPositionCommandHandler(IAppDbContext db, ICurrentUserService currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task<Guid> Handle(AddManualPositionCommand request, CancellationToken ct)
    {
        var userId = _currentUser.UserId
            ?? throw new DomainException("unauthorized", "Authentication required.");

        var symbol = request.Symbol.Trim().ToUpperInvariant();

        var asset = await _db.Assets.FirstOrDefaultAsync(a => a.Symbol == symbol, ct)
            ?? throw new DomainException("asset_not_found", $"Asset '{symbol}' not found.");

        var portfolio = await _db.Portfolios.FirstOrDefaultAsync(p => p.UserId == userId, ct)
            ?? throw new DomainException("portfolio_missing", "Portfolio not found.");

        var position = await _db.PortfolioPositions
            .FirstOrDefaultAsync(p => p.PortfolioId == portfolio.Id && p.AssetId == asset.Id, ct);
        if (position is null)
        {
            // Yeni pozisyon: constructor ile qty ve cost set, sonra UpdateManual ile tarih/not
            position = new PortfolioPosition(
                portfolio.Id, asset.Id,
                request.Quantity, request.AverageCost);

            position.UpdateManual(
                request.Quantity, request.AverageCost,
                request.PurchaseDate, request.Notes);

            _db.PortfolioPositions.Add(position);
        }
        else
        {
            // Mevcut pozisyon: ağırlıklı ortalama ile birleştir
            position.MergeManual(
                request.Quantity, request.AverageCost,
                request.PurchaseDate, request.Notes);
        }

        await _db.SaveChangesAsync(ct);
        return position.Id;
    }
}