using MediatR;
using Microsoft.EntityFrameworkCore;
using TradingApp.Application.Abstractions;
using TradingApp.Application.Features.Watchlist.Dtos;
using TradingApp.Domain.Common;

namespace TradingApp.Application.Features.Watchlist.GetWatchlist;

public sealed class GetWatchlistQueryHandler
    : IRequestHandler<GetWatchlistQuery, IReadOnlyCollection<WatchlistItemDto>>
{
    private readonly IAppDbContext _db;
    private readonly ICurrentUserService _currentUser;

    public GetWatchlistQueryHandler(IAppDbContext db, ICurrentUserService currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task<IReadOnlyCollection<WatchlistItemDto>> Handle(
        GetWatchlistQuery request, CancellationToken ct)
    {
        var userId = _currentUser.UserId
            ?? throw new DomainException("unauthorized", "Authentication required.");

        var rows = await (
            from w in _db.WatchlistItems.AsNoTracking()
            join a in _db.Assets.AsNoTracking() on w.AssetId equals a.Id
            where w.UserId == userId
            orderby w.CreatedAt descending
            select new
            {
                a.Id, a.Symbol, a.Name, a.AssetType, a.Currency,
                a.CurrentPrice, a.PreviousClose, w.CreatedAt
            }).ToListAsync(ct);

        return rows.Select(r => new WatchlistItemDto(
            r.Id,
            r.Symbol,
            r.Name,
            r.AssetType.ToString(),
            r.Currency,
            r.CurrentPrice,
            r.PreviousClose == 0m ? 0m
                : Math.Round((r.CurrentPrice - r.PreviousClose) / r.PreviousClose * 100m, 4),
            r.CreatedAt)).ToArray();
    }
}