using TradingApp.Domain.Common;
using TradingApp.Domain.Enums;

namespace TradingApp.Domain.Entities;

public sealed class PriceAlert : BaseEntity, IAggregateRoot
{
    public Guid UserId { get; private set; }
    public Guid AssetId { get; private set; }
    public PriceAlertCondition Condition { get; private set; }
    public decimal TargetPrice { get; private set; }
    public bool IsActive { get; private set; } = true;
    public DateTimeOffset? TriggeredAt { get; private set; }

    private PriceAlert() { } // EF

    public PriceAlert(Guid userId, Guid assetId, PriceAlertCondition condition, decimal targetPrice)
    {
        if (targetPrice <= 0) throw new DomainException("invalid_price", "Target price must be positive.");
        UserId = userId;
        AssetId = assetId;
        Condition = condition;
        TargetPrice = targetPrice;
    }

    public bool IsTriggered(decimal currentPrice) => Condition switch
    {
        PriceAlertCondition.GreaterThan => currentPrice > TargetPrice,
        PriceAlertCondition.LessThan => currentPrice < TargetPrice,
        _ => false
    };

    public void MarkTriggered()
    {
        IsActive = false;
        TriggeredAt = DateTimeOffset.UtcNow;
        Touch();
    }
}