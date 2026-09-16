using MediatR;
using Microsoft.EntityFrameworkCore;
using TradingApp.Application.Abstractions;
using TradingApp.Application.Features.Orders.Dtos;
using TradingApp.Domain.Common;
using TradingApp.Domain.Enums;

namespace TradingApp.Application.Features.Orders.GetOrders;

public sealed class GetOrdersQueryHandler
    : IRequestHandler<GetOrdersQuery, IReadOnlyCollection<OrderDto>>
{
    private readonly IAppDbContext _db;
    private readonly ICurrentUserService _currentUser;

    public GetOrdersQueryHandler(IAppDbContext db, ICurrentUserService currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task<IReadOnlyCollection<OrderDto>> Handle(
        GetOrdersQuery request, CancellationToken ct)
    {
        var userId = _currentUser.UserId
            ?? throw new DomainException("unauthorized", "Authentication required.");

        var query = _db.Orders.AsNoTracking().Where(o => o.UserId == userId);

        if (!string.IsNullOrWhiteSpace(request.Status)
            && Enum.TryParse<OrderStatus>(request.Status, true, out var status))
        {
            query = query.Where(o => o.Status == status);
        }

        if (!string.IsNullOrWhiteSpace(request.Symbol))
        {
            var symbol = request.Symbol.Trim().ToUpperInvariant();
            var assetIds = await _db.Assets
                .Where(a => a.Symbol == symbol)
                .Select(a => a.Id)
                .ToListAsync(ct);
            query = query.Where(o => assetIds.Contains(o.AssetId));
        }

        var orders = await query
            .OrderByDescending(o => o.CreatedAt)
            .Take(200)
            .ToListAsync(ct);

        var assetIdsAll = orders.Select(o => o.AssetId).Distinct().ToList();
        var symbols = await _db.Assets
            .Where(a => assetIdsAll.Contains(a.Id))
            .ToDictionaryAsync(a => a.Id, a => a.Symbol, ct);

        return orders.Select(o => new OrderDto(
            o.Id,
            symbols.GetValueOrDefault(o.AssetId, "?"),
            o.Side.ToString(),
            o.OrderType.ToString(),
            o.Status.ToString(),
            o.Price,
            o.Quantity,
            o.FilledQuantity,
            o.RemainingQuantity,
            o.AverageFillPrice,
            o.CreatedAt)).ToArray();
    }
}