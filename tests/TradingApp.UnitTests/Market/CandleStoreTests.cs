using TradingApp.Infrastructure.MarketData;

namespace TradingApp.UnitTests.Market;

public class CandleStoreTests
{
    [Fact]
    public async Task AppendTick_And_GetCandles_Returns_Bucketed_Data()
    {
        var store = new CandleStore();

        for (int i = 0; i < 10; i++)
            await store.AppendTickAsync("BTC", 100m + i, 1m);

        var candles = await store.GetCandlesAsync("BTC", "1m", 10);

        Assert.NotEmpty(candles);
        var first = candles.First();
        Assert.True(first.High >= first.Low);
        Assert.True(first.Close >= first.Low);
    }

    [Fact]
    public async Task Unknown_Symbol_Returns_Empty()
    {
        var store = new CandleStore();
        var candles = await store.GetCandlesAsync("XXX", "1m", 10);
        Assert.Empty(candles);
    }
}