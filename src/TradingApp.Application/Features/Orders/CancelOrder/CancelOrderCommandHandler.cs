using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TradingApp.Application.Abstractions;
using TradingApp.Domain.Common;
using TradingApp.Domain.Enums;

namespace TradingApp.Application.Features.Orders.CancelOrder;

public sealed class CancelOrderCommandHandler : IRequestHandler<CancelOrderCommand>
{
    private readonly IAppDbContext _db;
    private readonly ICurrentUserService _currentUser;
    private readonly ILogger<CancelOrderCommandHandler> _logger;

    public CancelOrderCommandHandler(
        IAppDbContext db,
        ICurrentUserService currentUser,
        ILogger<CancelOrderCommandHandler> logger)
    {
        _db = db;
        _currentUser = currentUser;
        _logger = logger;
    }

    public async Task Handle(CancelOrderCommand request, CancellationToken ct)
    {
        var userId = _currentUser.UserId
            ?? throw new DomainException("unauthorized", "Authentication required.");

        var order = await _db.Orders
            .FirstOrDefaultAsync(o => o.Id == request.OrderId, ct)
            ?? throw new DomainException("order_not_found", "Order not found.");

        if (order.UserId != userId)
            throw new DomainException("forbidden", "You can only cancel your own orders.");

        if (order.Status is OrderStatus.Filled or OrderStatus.Cancelled)
            throw new DomainException("invalid_state", $"Order is already {order.Status}.");

        var asset = await _db.Assets.FirstAsync(a => a.Id == order.AssetId, ct);
        var remaining = order.RemainingQuantity;

        // Kilitleri çöz
        if (order.Side == OrderSide.Buy)
        {
            var wallet = await _db.Wallets
                .FirstAsync(w => w.UserId == userId && w.Currency == asset.Currency, ct);
            var unlockAmount = order.LockedPrice * remaining;
            wallet.Unlock(unlockAmount);
        }
        else // Sell
        {
            var portfolio = await _db.Portfolios
                .FirstAsync(p => p.UserId == userId, ct);
            var position = await _db.PortfolioPositions
                .FirstAsync(p => p.PortfolioId == portfolio.Id && p.AssetId == order.AssetId, ct);
            position.UnlockFromSell(remaining);
        }

        order.Cancel();
        await _db.SaveChangesAsync(ct);

        _logger.LogInformation("Order cancelled: {OrderId}", order.Id);
    }
}