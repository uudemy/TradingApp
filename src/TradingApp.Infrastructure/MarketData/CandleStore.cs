using System.Collections.Concurrent;
using TradingApp.Application.Abstractions;
using TradingApp.Application.Features.Market.Dtos;

namespace TradingApp.Infrastructure.MarketData;

/// <summary>
/// In-memory candle store. Her sembol için son ~500 tick tutar.
/// Tick'ler 1 saniyelik; istek interval'e göre bucket'lanır.
/// Thread-safe (ConcurrentDictionary + per-symbol lock).
/// </summary>
public sealed class CandleStore : ICandleStore
{
    private const int MaxTicks = 500;

    private sealed record Tick(DateTimeOffset Time, decimal Price, decimal Volume);

    private readonly ConcurrentDictionary<string, List<Tick>> _ticks = new();
    private readonly ConcurrentDictionary<string, object> _locks = new();

    public Task AppendTickAsync(string symbol, decimal price, decimal volume, CancellationToken ct = default)
    {
        symbol = symbol.ToUpperInvariant();
        var list = _ticks.GetOrAdd(symbol, _ => new List<Tick>());
        var gate = _locks.GetOrAdd(symbol, _ => new object());

        lock (gate)
        {
            list.Add(new Tick(DateTimeOffset.UtcNow, price, volume));
            if (list.Count > MaxTicks)
                list.RemoveRange(0, list.Count - MaxTicks);
        }
        return Task.CompletedTask;
    }

    public Task<IReadOnlyCollection<CandleDto>> GetCandlesAsync(
        string symbol, string interval, int limit, CancellationToken ct = default)
    {
        symbol = symbol.ToUpperInvariant();
        if (!_ticks.TryGetValue(symbol, out var ticks))
            return Task.FromResult<IReadOnlyCollection<CandleDto>>(Array.Empty<CandleDto>());

        var bucket = ParseInterval(interval);
        Tick[] snapshot;
        lock (_locks.GetOrAdd(symbol, _ => new object()))
            snapshot = ticks.ToArray();

        var grouped = snapshot
            .GroupBy(t => new DateTimeOffset(
                t.Time.UtcDateTime.AddTicks(-(t.Time.UtcDateTime.Ticks % bucket.Ticks)),
                TimeSpan.Zero))
            .OrderBy(g => g.Key)
            .TakeLast(Math.Clamp(limit, 1, 500))
            .Select(g => new CandleDto(
                g.Key,
                g.First().Price,
                g.Max(x => x.Price),
                g.Min(x => x.Price),
                g.Last().Price,
                g.Sum(x => x.Volume)))
            .ToArray();

        return Task.FromResult<IReadOnlyCollection<CandleDto>>(grouped);
    }

    private static TimeSpan ParseInterval(string interval) => interval switch
    {
        "1s"  => TimeSpan.FromSeconds(1),
        "1m"  => TimeSpan.FromMinutes(1),
        "5m"  => TimeSpan.FromMinutes(5),
        "15m" => TimeSpan.FromMinutes(15),
        "1h"  => TimeSpan.FromHours(1),
        "4h"  => TimeSpan.FromHours(4),
        "1D"  => TimeSpan.FromDays(1),
        _     => TimeSpan.FromMinutes(1)
    };
}