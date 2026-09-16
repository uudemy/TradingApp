using TradingApp.Domain.Common;
using TradingApp.Domain.Entities;

namespace TradingApp.UnitTests.Domain;

public class PortfolioPositionTests
{
    [Fact]
    public void ApplyBuy_Should_Recalculate_Weighted_Average()
    {
        var p = new PortfolioPosition(Guid.NewGuid(), Guid.NewGuid(), 0m, 0m);
        p.ApplyBuy(100m, 10m);
        p.ApplyBuy(100m, 20m);

        Assert.Equal(200m, p.Quantity);
        Assert.Equal(15m, p.AverageCost);  // (100*10 + 100*20)/200
    }

    [Fact]
    public void ApplySell_Should_Compute_Realized_PnL()
    {
        var p = new PortfolioPosition(Guid.NewGuid(), Guid.NewGuid(), 0m, 0m);
        p.ApplyBuy(100m, 10m);
        p.ApplySell(40m, 25m);

        Assert.Equal(60m, p.Quantity);
        Assert.Equal(600m, p.RealizedPnl);  // (25-10)*40
        Assert.Equal(10m, p.AverageCost);
    }

    [Fact]
    public void ApplySell_Exceeding_Quantity_Should_Throw()
    {
        var p = new PortfolioPosition(Guid.NewGuid(), Guid.NewGuid(), 0m, 0m);
        p.ApplyBuy(10m, 10m);

        Assert.Throws<DomainException>(() => p.ApplySell(20m, 15m));
    }
}