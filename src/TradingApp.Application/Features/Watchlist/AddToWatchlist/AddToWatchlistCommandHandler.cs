using MediatR;
using Microsoft.EntityFrameworkCore;
using TradingApp.Application.Abstractions;
using TradingApp.Domain.Common;
using TradingApp.Domain.Entities;

namespace TradingApp.Application.Features.Watchlist.AddToWatchlist;

public sealed class AddToWatchlistCommandHandler : IRequestHandler<AddToWatchlistCommand>
{
    private readonly IAppDbContext _db;
    private readonly ICurrentUserService _currentUser;

    public AddToWatchlistCommandHandler(IAppDbContext db, ICurrentUserService currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task Handle(AddToWatchlistCommand request, CancellationToken ct)
    {
        var userId = _currentUser.UserId
            ?? throw new DomainException("unauthorized", "Authentication required.");

        var symbol = request.Symbol.Trim().ToUpperInvariant();

        var asset = await _db.Assets.FirstOrDefaultAsync(a => a.Symbol == symbol, ct)
            ?? throw new DomainException("asset_not_found", $"Asset '{symbol}' not found.");

        var exists = await _db.WatchlistItems
            .AnyAsync(w => w.UserId == userId && w.AssetId == asset.Id, ct);

        if (exists)
            throw new DomainException("already_in_watchlist", $"{symbol} already in watchlist.");

        _db.WatchlistItems.Add(new WatchlistItem(userId, asset.Id));
        await _db.SaveChangesAsync(ct);
    }
}