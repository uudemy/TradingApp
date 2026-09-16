using Microsoft.EntityFrameworkCore;
using TradingApp.Application.Features.Market.Dtos;
using TradingApp.Application.Features.Market.GetAssets;
using TradingApp.Domain.Entities;
using TradingApp.Domain.Enums;
using TradingApp.Infrastructure.Persistence;

namespace TradingApp.UnitTests.Market;

public class GetAssetsQueryHandlerTests
{
    private sealed class FakeCache : Application.Abstractions.IMarketDataCache
    {
        public IReadOnlyCollection<AssetDto>? List { get; private set; }
        public bool SetCalled { get; private set; }

        public Task<IReadOnlyCollection<AssetDto>?> GetAssetListAsync(CancellationToken ct = default)
            => Task.FromResult(List);

        public Task SetAssetListAsync(IReadOnlyCollection<AssetDto> assets, TimeSpan ttl, CancellationToken ct = default)
        {
            List = assets;
            SetCalled = true;
            return Task.CompletedTask;
        }

        public Task<MarketPriceDto?> GetPriceAsync(string symbol, CancellationToken ct = default)
            => Task.FromResult<MarketPriceDto?>(null);

        public Task SetPriceAsync(MarketPriceDto price, TimeSpan ttl, CancellationToken ct = default)
            => Task.CompletedTask;

        public Task InvalidateAssetListAsync(CancellationToken ct = default) => Task.CompletedTask;
    }

    private static AppDbContext CreateDb()
    {
        var opts = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new AppDbContext(opts);
    }

    [Fact]
    public async Task Handler_Computes_Change_Percent()
    {
        await using var db = CreateDb();
        db.Assets.Add(new Asset("BTC", "Bitcoin", AssetType.Crypto, "USDT", 100m));
        await db.SaveChangesAsync();

        var cache = new FakeCache();
        var handler = new GetAssetsQueryHandler(db, cache);

        var result = await handler.Handle(new GetAssetsQuery(), CancellationToken.None);

        Assert.Single(result);
        Assert.Equal("BTC", result.First().Symbol);
        Assert.True(cache.SetCalled);
    }
}