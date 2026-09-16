using MediatR;
using Microsoft.EntityFrameworkCore;
using TradingApp.Application.Abstractions;
using TradingApp.Application.Features.Orders.Dtos;
using TradingApp.Domain.Common;

namespace TradingApp.Application.Features.Trades.GetTrades;

public sealed class GetTradesQueryHandler
    : IRequestHandler<GetTradesQuery, IReadOnlyCollection<TradeDto>>
{
    private readonly IAppDbContext _db;
    private readonly ICurrentUserService _currentUser;

    public GetTradesQueryHandler(IAppDbContext db, ICurrentUserService currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task<IReadOnlyCollection<TradeDto>> Handle(
        GetTradesQuery request, CancellationToken ct)
    {
        var userId = _currentUser.UserId
            ?? throw new DomainException("unauthorized", "Authentication required.");

        var query = _db.Trades.AsNoTracking()
            .Where(t => t.BuyerUserId == userId || t.SellerUserId == userId);

        if (!string.IsNullOrWhiteSpace(request.Symbol))
        {
            var symbol = request.Symbol.Trim().ToUpperInvariant();
            var assetIds = await _db.Assets
                .Where(a => a.Symbol == symbol)
                .Select(a => a.Id)
                .ToListAsync(ct);
            query = query.Where(t => assetIds.Contains(t.AssetId));
        }

        var trades = await query
            .OrderByDescending(t => t.CreatedAt)
            .Take(200)
            .ToListAsync(ct);

        var assetIdsAll = trades.Select(t => t.AssetId).Distinct().ToList();
        var symbols = await _db.Assets
            .Where(a => assetIdsAll.Contains(a.Id))
            .ToDictionaryAsync(a => a.Id, a => a.Symbol, ct);

        return trades.Select(t => new TradeDto(
            t.Id,
            symbols.GetValueOrDefault(t.AssetId, "?"),
            t.Price,
            t.Quantity,
            t.Price * t.Quantity,
            t.BuyOrderId,
            t.SellOrderId,
            t.CreatedAt)).ToArray();
    }
}