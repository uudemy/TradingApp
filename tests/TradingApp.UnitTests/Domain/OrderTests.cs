using TradingApp.Domain.Common;
using TradingApp.Domain.Entities;
using TradingApp.Domain.Enums;

namespace TradingApp.UnitTests.Domain;

public class OrderTests
{
    [Fact]
    public void Partial_Fill_Should_Update_Status_And_Average_Price()
    {
        var o = new Order(
            Guid.NewGuid(), Guid.NewGuid(),
            OrderSide.Buy, OrderType.Limit,
            price: 100m, quantity: 100m, lockedPrice: 100m);

        o.Fill(40m, 99m);
        o.Fill(30m, 101m);

        Assert.Equal(70m, o.FilledQuantity);
        Assert.Equal(30m, o.RemainingQuantity);
        Assert.Equal(OrderStatus.PartiallyFilled, o.Status);
        // weighted avg: (40*99 + 30*101) / 70 = (3960 + 3030)/70 = 99.857...
        Assert.Equal(99.85714286m, Math.Round(o.AverageFillPrice, 8));
    }

    [Fact]
    public void Full_Fill_Should_Mark_Status_Filled()
    {
        var o = new Order(
            Guid.NewGuid(), Guid.NewGuid(),
            OrderSide.Sell, OrderType.Limit,
            price: 100m, quantity: 100m, lockedPrice: 100m);

        o.Fill(100m, 100m);

        Assert.Equal(OrderStatus.Filled, o.Status);
        Assert.Equal(0m, o.RemainingQuantity);
    }

    [Fact]
    public void Over_Fill_Should_Throw()
    {
        var o = new Order(
            Guid.NewGuid(), Guid.NewGuid(),
            OrderSide.Buy, OrderType.Limit,
            price: 100m, quantity: 50m, lockedPrice: 100m);

        Assert.Throws<DomainException>(() => o.Fill(100m, 100m));
    }

    [Fact]
    public void Market_Order_With_Non_Zero_Price_Should_Throw()
    {
        Assert.Throws<DomainException>(() =>
            new Order(
                Guid.NewGuid(), Guid.NewGuid(),
                OrderSide.Buy, OrderType.Market,
                price: 5m, quantity: 10m, lockedPrice: 100m));
    }

    [Fact]
    public void Market_Order_With_Zero_Price_And_Positive_LockedPrice_Should_Succeed()
    {
        var o = new Order(
            Guid.NewGuid(), Guid.NewGuid(),
            OrderSide.Buy, OrderType.Market,
            price: 0m, quantity: 10m, lockedPrice: 100m);

        Assert.Equal(OrderType.Market, o.OrderType);
        Assert.Equal(0m, o.Price);
        Assert.Equal(100m, o.LockedPrice);
        Assert.Equal(OrderStatus.Open, o.Status);
    }
}