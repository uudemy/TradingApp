using TradingApp.Domain.Common;

namespace TradingApp.Domain.Entities;

/// <summary>Gerçekleşen eşleşme. Değiştirilemez — sadece insert.</summary>
public sealed class Trade : BaseEntity, IAggregateRoot
{
    public Guid BuyOrderId { get; private set; }
    public Guid SellOrderId { get; private set; }
    public Guid AssetId { get; private set; }
    public Guid BuyerUserId { get; private set; }
    public Guid SellerUserId { get; private set; }
    public decimal Price { get; private set; }
    public decimal Quantity { get; private set; }

    private Trade() { } // EF

    public Trade(
        Guid buyOrderId, Guid sellOrderId, Guid assetId,
        Guid buyerUserId, Guid sellerUserId,
        decimal price, decimal quantity)
    {
        if (price <= 0) throw new DomainException("invalid_price", "Trade price must be positive.");
        if (quantity <= 0) throw new DomainException("invalid_quantity", "Trade quantity must be positive.");

        BuyOrderId = buyOrderId;
        SellOrderId = sellOrderId;
        AssetId = assetId;
        BuyerUserId = buyerUserId;
        SellerUserId = sellerUserId;
        Price = price;
        Quantity = quantity;
    }
}