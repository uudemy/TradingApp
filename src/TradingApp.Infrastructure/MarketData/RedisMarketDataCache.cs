using System.Text.Json;
using Microsoft.Extensions.Caching.Distributed;
using TradingApp.Application.Abstractions;
using TradingApp.Application.Features.Market.Dtos;

namespace TradingApp.Infrastructure.MarketData;

public sealed class RedisMarketDataCache : IMarketDataCache
{
    private const string AssetListKey = "market:assets:list";
    private static string PriceKey(string symbol) => $"market:price:{symbol}";

    private static readonly JsonSerializerOptions JsonOpts = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    private readonly IDistributedCache _cache;
    public RedisMarketDataCache(IDistributedCache cache) => _cache = cache;

    public async Task<IReadOnlyCollection<AssetDto>?> GetAssetListAsync(CancellationToken ct = default)
        => await GetAsync<AssetDto[]>(AssetListKey, ct);

    public async Task SetAssetListAsync(IReadOnlyCollection<AssetDto> assets, TimeSpan ttl, CancellationToken ct = default)
        => await SetAsync(AssetListKey, assets.ToArray(), ttl, ct);

    public async Task<MarketPriceDto?> GetPriceAsync(string symbol, CancellationToken ct = default)
        => await GetAsync<MarketPriceDto>(PriceKey(symbol.ToUpperInvariant()), ct);

    public async Task SetPriceAsync(MarketPriceDto price, TimeSpan ttl, CancellationToken ct = default)
        => await SetAsync(PriceKey(price.Symbol.ToUpperInvariant()), price, ttl, ct);

    public Task InvalidateAssetListAsync(CancellationToken ct = default)
        => _cache.RemoveAsync(AssetListKey, ct);

    private async Task<T?> GetAsync<T>(string key, CancellationToken ct)
    {
        var bytes = await _cache.GetAsync(key, ct);
        if (bytes is null || bytes.Length == 0) return default;
        return JsonSerializer.Deserialize<T>(bytes, JsonOpts);
    }

    private async Task SetAsync<T>(string key, T value, TimeSpan ttl, CancellationToken ct)
    {
        var bytes = JsonSerializer.SerializeToUtf8Bytes(value, JsonOpts);
        await _cache.SetAsync(key, bytes, new DistributedCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = ttl
        }, ct);
    }
}