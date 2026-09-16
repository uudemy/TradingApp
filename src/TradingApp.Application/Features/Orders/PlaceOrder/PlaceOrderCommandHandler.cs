using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TradingApp.Application.Abstractions;
using TradingApp.Application.Features.Orders.Dtos;
using TradingApp.Domain.Common;
using TradingApp.Domain.Entities;
using TradingApp.Domain.Enums;

namespace TradingApp.Application.Features.Orders.PlaceOrder;

public sealed class PlaceOrderCommandHandler : IRequestHandler<PlaceOrderCommand, OrderDto>
{
    private readonly IAppDbContext _db;
    private readonly IOrderMatchingEngine _engine;
    private readonly ICurrentUserService _currentUser;
    private readonly ILogger<PlaceOrderCommandHandler> _logger;

    public PlaceOrderCommandHandler(
        IAppDbContext db,
        IOrderMatchingEngine engine,
        ICurrentUserService currentUser,
        ILogger<PlaceOrderCommandHandler> logger)
    {
        _db = db;
        _engine = engine;
        _currentUser = currentUser;
        _logger = logger;
    }

    public async Task<OrderDto> Handle(PlaceOrderCommand request, CancellationToken ct)
    {
        var userId = _currentUser.UserId
            ?? throw new DomainException("unauthorized", "Authentication required.");

        var symbol = request.Symbol.Trim().ToUpperInvariant();

        var asset = await _db.Assets.FirstOrDefaultAsync(a => a.Symbol == symbol, ct)
            ?? throw new DomainException("asset_not_found", $"Asset '{symbol}' not found.");

        if (!asset.IsActive)
            throw new DomainException("asset_inactive", "Asset is not active.");

        // Fiyatı belirle
        var orderPrice = request.OrderType == OrderType.Market
            ? asset.CurrentPrice
            : request.Price!.Value;

        if (request.OrderType == OrderType.Market && orderPrice <= 0m)
            throw new DomainException("no_market_price", "Market price unavailable.");

        // Bakiye / pozisyon kilitle
        if (request.Side == OrderSide.Buy)
        {
            var required = orderPrice * request.Quantity;
            var wallet = await _db.Wallets
                .FirstOrDefaultAsync(w => w.UserId == userId && w.Currency == asset.Currency, ct)
                ?? throw new DomainException("wallet_missing",
                    $"No {asset.Currency} wallet for user.");

            wallet.Lock(required);
        }
        else // Sell
        {
            var portfolio = await _db.Portfolios
                .FirstOrDefaultAsync(p => p.UserId == userId, ct)
                ?? throw new DomainException("portfolio_missing", "Portfolio not found.");

            var position = await _db.PortfolioPositions
                .FirstOrDefaultAsync(p => p.PortfolioId == portfolio.Id && p.AssetId == asset.Id, ct);

            if (position is null || position.AvailableQuantity < request.Quantity)
                throw new DomainException("insufficient_position",
                    "Not enough asset quantity to sell.");

            position.LockForSell(request.Quantity);
        }

        // Emri oluştur ve kaydet
        var order = new Order(
            userId,
            asset.Id,
            request.Side,
            request.OrderType,
            request.OrderType == OrderType.Market ? 0m : request.Price!.Value,
            request.Quantity,
            lockedPrice: orderPrice);

        _db.Orders.Add(order);
        await _db.SaveChangesAsync(ct);

        _logger.LogInformation(
            "Order created: {OrderId} {Side} {Qty} {Symbol} @ {Price}",
            order.Id, order.Side, order.Quantity, symbol, order.Price);

        // Eşleştir
        await _engine.MatchAsync(order, ct);

        // Güncel halini oku
        var reloaded = await _db.Orders.AsNoTracking()
            .FirstAsync(o => o.Id == order.Id, ct);

        return new OrderDto(
            reloaded.Id,
            symbol,
            reloaded.Side.ToString(),
            reloaded.OrderType.ToString(),
            reloaded.Status.ToString(),
            reloaded.Price,
            reloaded.Quantity,
            reloaded.FilledQuantity,
            reloaded.RemainingQuantity,
            reloaded.AverageFillPrice,
            reloaded.CreatedAt);
    }
}