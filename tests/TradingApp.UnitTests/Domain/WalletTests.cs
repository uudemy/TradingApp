using TradingApp.Domain.Common;
using TradingApp.Domain.Entities;

namespace TradingApp.UnitTests.Domain;

public class WalletTests
{
    [Fact]
    public void Lock_Should_Move_From_Available_To_Locked()
    {
        var w = new Wallet(Guid.NewGuid(), "TRY", 1000m);
        w.Lock(300m);

        Assert.Equal(700m, w.AvailableBalance);
        Assert.Equal(300m, w.LockedBalance);
    }

    [Fact]
    public void Lock_Should_Throw_When_Insufficient()
    {
        var w = new Wallet(Guid.NewGuid(), "TRY", 100m);

        var ex = Assert.Throws<DomainException>(() => w.Lock(200m));
        Assert.Equal("insufficient_balance", ex.Code);
    }

    [Fact]
    public void Unlock_Should_Return_To_Available()
    {
        var w = new Wallet(Guid.NewGuid(), "TRY", 1000m);
        w.Lock(300m);
        w.Unlock(150m);

        Assert.Equal(850m, w.AvailableBalance);
        Assert.Equal(150m, w.LockedBalance);
    }
}