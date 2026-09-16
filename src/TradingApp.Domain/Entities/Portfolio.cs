using TradingApp.Domain.Common;

namespace TradingApp.Domain.Entities;

public sealed class Portfolio : BaseEntity, IAggregateRoot
{
    public Guid UserId { get; private set; }
    public ICollection<PortfolioPosition> Positions { get; private set; } = new List<PortfolioPosition>();

    private Portfolio() { } // EF

    public Portfolio(Guid userId) { UserId = userId; }
}