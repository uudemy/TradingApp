using MediatR;
using Microsoft.EntityFrameworkCore;
using TradingApp.Application.Abstractions;
using TradingApp.Domain.Common;

namespace TradingApp.Application.Features.Portfolio.RemovePosition;

public sealed class RemovePositionCommandHandler : IRequestHandler<RemovePositionCommand>
{
    private readonly IAppDbContext _db;
    private readonly ICurrentUserService _currentUser;

    public RemovePositionCommandHandler(IAppDbContext db, ICurrentUserService currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task Handle(RemovePositionCommand request, CancellationToken ct)
    {
        var userId = _currentUser.UserId
            ?? throw new DomainException("unauthorized", "Authentication required.");

        var position = await _db.PortfolioPositions
            .FirstOrDefaultAsync(p => p.Id == request.PositionId, ct)
            ?? throw new DomainException("position_not_found", "Position not found.");

        if (position.LockedQuantity > 0m)
            throw new DomainException("position_locked",
                "Cannot remove a position with active locked quantity.");

        var portfolio = await _db.Portfolios
            .FirstOrDefaultAsync(p => p.Id == position.PortfolioId, ct)
            ?? throw new DomainException("portfolio_missing", "Portfolio not found.");

        if (portfolio.UserId != userId)
            throw new DomainException("forbidden", "You can only remove your own positions.");

        _db.PortfolioPositions.Remove(position);
        await _db.SaveChangesAsync(ct);
    }
}