using TradingApp.Domain.Common;

namespace TradingApp.Domain.Entities;

public sealed class WatchlistItem : BaseEntity
{
    public Guid UserId { get; private set; }
    public Guid AssetId { get; private set; }

    private WatchlistItem() { } // EF

    public WatchlistItem(Guid userId, Guid assetId)
    {
        UserId = userId;
        AssetId = assetId;
    }
}