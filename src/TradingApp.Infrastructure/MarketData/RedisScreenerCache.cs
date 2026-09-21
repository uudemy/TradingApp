using System.Text.Json;
using Microsoft.Extensions.Caching.Distributed;
using TradingApp.Application.Abstractions;
using TradingApp.Application.Features.Analysis.Dtos;

namespace TradingApp.Infrastructure.MarketData;

public sealed class RedisScreenerCache : IScreenerCache
{
    private const string Prefix = "screener:result:";

    private static readonly JsonSerializerOptions JsonOpts = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    private readonly IDistributedCache _cache;
    public RedisScreenerCache(IDistributedCache cache) => _cache = cache;

    public async Task<ScreenerResultDto?> GetAsync(string key, CancellationToken ct = default)
    {
        var bytes = await _cache.GetAsync(Prefix + key, ct);
        if (bytes is null || bytes.Length == 0) return null;
        return JsonSerializer.Deserialize<ScreenerResultDto>(bytes, JsonOpts);
    }

    public async Task SetAsync(string key, ScreenerResultDto result, TimeSpan ttl, CancellationToken ct = default)
    {
        var bytes = JsonSerializer.SerializeToUtf8Bytes(result, JsonOpts);
        await _cache.SetAsync(Prefix + key, bytes, new DistributedCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = ttl
        }, ct);
    }
}