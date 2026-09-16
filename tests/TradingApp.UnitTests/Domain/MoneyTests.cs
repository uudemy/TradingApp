using TradingApp.Domain.Common;
using TradingApp.Domain.ValueObjects;

namespace TradingApp.UnitTests.Domain;

public class MoneyTests
{
    [Fact]
    public void Add_Same_Currency_Works()
    {
        var a = new Money(10.5m, "TRY");
        var b = new Money(2.25m, "TRY");

        var result = a.Add(b);
        Assert.Equal(12.75m, result.Amount);
        Assert.Equal("TRY", result.Currency);
    }

    [Fact]
    public void Add_Different_Currency_Throws()
    {
        var a = new Money(10m, "TRY");
        var b = new Money(10m, "USD");

        Assert.Throws<DomainException>(() => a.Add(b));
    }

    [Fact]
    public void Currency_Normalizes_To_Uppercase()
    {
        var m = new Money(1m, "try");
        Assert.Equal("TRY", m.Currency);
    }
}