using MediatR;
using Microsoft.EntityFrameworkCore;
using TradingApp.Application.Abstractions;
using TradingApp.Domain.Common;

namespace TradingApp.Application.Features.Portfolio.UpdatePosition;

public sealed class UpdatePositionCommandHandler : IRequestHandler<UpdatePositionCommand>
{
    private readonly IAppDbContext _db;
    private readonly ICurrentUserService _currentUser;

    public UpdatePositionCommandHandler(IAppDbContext db, ICurrentUserService currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task Handle(UpdatePositionCommand request, CancellationToken ct)
    {
        var userId = _currentUser.UserId
            ?? throw new DomainException("unauthorized", "Authentication required.");

        var position = await _db.PortfolioPositions
            .FirstOrDefaultAsync(p => p.Id == request.PositionId, ct)
            ?? throw new DomainException("position_not_found", "Position not found.");

        var portfolio = await _db.Portfolios
            .FirstOrDefaultAsync(p => p.Id == position.PortfolioId, ct)
            ?? throw new DomainException("portfolio_missing", "Portfolio not found.");

        if (portfolio.UserId != userId)
            throw new DomainException("forbidden", "You can only update your own positions.");

        position.UpdateManual(
            request.Quantity, request.AverageCost,
            request.PurchaseDate, request.Notes);

        await _db.SaveChangesAsync(ct);
    }
}