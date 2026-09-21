using MediatR;
using Microsoft.EntityFrameworkCore;
using TradingApp.Application.Abstractions;
using TradingApp.Domain.Common;

namespace TradingApp.Application.Features.Watchlist.RemoveFromWatchlist;

public sealed class RemoveFromWatchlistCommandHandler : IRequestHandler<RemoveFromWatchlistCommand>
{
    private readonly IAppDbContext _db;
    private readonly ICurrentUserService _currentUser;

    public RemoveFromWatchlistCommandHandler(IAppDbContext db, ICurrentUserService currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task Handle(RemoveFromWatchlistCommand request, CancellationToken ct)
    {
        var userId = _currentUser.UserId
            ?? throw new DomainException("unauthorized", "Authentication required.");

        var item = await _db.WatchlistItems
            .FirstOrDefaultAsync(w => w.UserId == userId && w.AssetId == request.AssetId, ct)
            ?? throw new DomainException("not_in_watchlist", "Item not in watchlist.");

        _db.WatchlistItems.Remove(item);
        await _db.SaveChangesAsync(ct);
    }
}