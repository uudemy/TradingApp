using TradingApp.Domain.Common;

namespace TradingApp.Domain.Entities;

public sealed class PortfolioPosition : BaseEntity
{
    public Guid PortfolioId { get; private set; }
    public Guid AssetId { get; private set; }
    public decimal Quantity { get; private set; }
    public decimal LockedQuantity { get; private set; }
    public decimal AverageCost { get; private set; }
    public decimal RealizedPnl { get; private set; }

    public decimal AvailableQuantity => Quantity - LockedQuantity;

    private PortfolioPosition() { } // EF

    public PortfolioPosition(Guid portfolioId, Guid assetId, decimal quantity, decimal averageCost)
    {
        if (quantity < 0) throw new DomainException("invalid_quantity", "Quantity cannot be negative.");
        if (averageCost < 0) throw new DomainException("invalid_cost", "Cost cannot be negative.");

        PortfolioId = portfolioId;
        AssetId = assetId;
        Quantity = quantity;
        AverageCost = averageCost;
    }

    /// <summary>Alış — weighted average cost günceller.</summary>
    public void ApplyBuy(decimal qty, decimal price)
    {
        if (qty <= 0) throw new DomainException("invalid_quantity", "Buy quantity must be positive.");
        if (price <= 0) throw new DomainException("invalid_price", "Buy price must be positive.");

        var newQty = Quantity + qty;
        AverageCost = newQty == 0m
            ? 0m
            : ((Quantity * AverageCost) + (qty * price)) / newQty;
        Quantity = newQty;
        Touch();
    }

    /// <summary>SELL emri verildiğinde miktarı kilitle.</summary>
    public void LockForSell(decimal qty)
    {
        if (qty <= 0) throw new DomainException("invalid_quantity", "Lock quantity must be positive.");
        if (AvailableQuantity < qty)
            throw new DomainException("insufficient_position",
                $"Insufficient position. Required: {qty}, Available: {AvailableQuantity}.");

        LockedQuantity += qty;
        Touch();
    }

    /// <summary>SELL emri iptal edildiğinde kilidi çöz.</summary>
    public void UnlockFromSell(decimal qty)
    {
        if (qty <= 0) throw new DomainException("invalid_quantity", "Unlock quantity must be positive.");
        if (LockedQuantity < qty)
            throw new DomainException("invalid_lock_state", "Unlock exceeds locked.");

        LockedQuantity -= qty;
        Touch();
    }

    /// <summary>SELL trade gerçekleştiğinde kilitli miktarı tüket, realized PnL hesapla.</summary>
    public void ApplySell(decimal qty, decimal price)
    {
        if (qty <= 0) throw new DomainException("invalid_quantity", "Sell quantity must be positive.");
        if (LockedQuantity < qty)
            throw new DomainException("insufficient_locked_position",
                $"Locked quantity {LockedQuantity} < sell quantity {qty}.");

        RealizedPnl += (price - AverageCost) * qty;
        Quantity -= qty;
        LockedQuantity -= qty;

        if (Quantity == 0m) AverageCost = 0m;
        Touch();
    }

    public decimal MarketValue(decimal currentPrice) => Quantity * currentPrice;
    public decimal UnrealizedPnl(decimal currentPrice) => (currentPrice - AverageCost) * Quantity;
}