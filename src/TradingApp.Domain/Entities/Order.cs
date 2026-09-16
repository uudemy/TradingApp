using TradingApp.Domain.Common;
using TradingApp.Domain.Enums;

namespace TradingApp.Domain.Entities;

public sealed class Order : BaseEntity, IAggregateRoot
{
    public Guid UserId { get; private set; }
    public Guid AssetId { get; private set; }
    public OrderSide Side { get; private set; }
    public OrderType OrderType { get; private set; }
    public OrderStatus Status { get; private set; }
    public decimal Price { get; private set; }
    public decimal Quantity { get; private set; }
    public decimal FilledQuantity { get; private set; }
    public decimal AverageFillPrice { get; private set; }

    /// <summary>
    /// Emir verilirken bakiye kilitlemede kullanılan birim fiyat.
    /// Market emirlerde anlık piyasa fiyatıdır.
    /// Limit emirlerde kullanıcının verdiği fiyattır.
    /// </summary>
    public decimal LockedPrice { get; private set; }

    public decimal RemainingQuantity => Quantity - FilledQuantity;

    public uint RowVersion { get; private set; }

    private Order() { } // EF

    public Order(
        Guid userId,
        Guid assetId,
        OrderSide side,
        OrderType orderType,
        decimal price,
        decimal quantity,
        decimal lockedPrice)
    {
        if (quantity <= 0) throw new DomainException("invalid_quantity", "Quantity must be positive.");
        if (orderType == OrderType.Limit && price <= 0)
            throw new DomainException("invalid_price", "Limit order price must be positive.");
        if (orderType == OrderType.Market && price != 0m)
            throw new DomainException("invalid_price", "Market order price must be 0.");
        if (lockedPrice <= 0)
            throw new DomainException("invalid_locked_price", "Locked price must be positive.");

        UserId = userId;
        AssetId = assetId;
        Side = side;
        OrderType = orderType;
        Price = price;
        Quantity = quantity;
        LockedPrice = lockedPrice;
        Status = OrderStatus.Open;
    }

    public void Fill(decimal fillQty, decimal fillPrice)
    {
        if (Status is OrderStatus.Cancelled or OrderStatus.Filled or OrderStatus.Rejected)
            throw new DomainException("invalid_state", $"Cannot fill order in {Status} state.");
        if (fillQty <= 0) throw new DomainException("invalid_quantity", "Fill quantity must be positive.");
        if (fillQty > RemainingQuantity)
            throw new DomainException("over_fill", $"Fill {fillQty} exceeds remaining {RemainingQuantity}.");

        var newFilled = FilledQuantity + fillQty;
        AverageFillPrice = newFilled == 0m
            ? 0m
            : ((FilledQuantity * AverageFillPrice) + (fillQty * fillPrice)) / newFilled;

        FilledQuantity = newFilled;
        Status = RemainingQuantity == 0m ? OrderStatus.Filled : OrderStatus.PartiallyFilled;
        Touch();
    }

    public void Cancel()
    {
        if (Status is OrderStatus.Filled or OrderStatus.Cancelled)
            throw new DomainException("invalid_state", $"Cannot cancel order in {Status} state.");

        Status = OrderStatus.Cancelled;
        Touch();
    }

    public void Reject(string reason)
    {
        if (Status == OrderStatus.Filled)
            throw new DomainException("invalid_state", "Cannot reject filled order.");
        Status = OrderStatus.Rejected;
        Touch();
    }
}