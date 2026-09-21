using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Skender.Stock.Indicators;
using System.Collections.Concurrent;
using TradingApp.Application.Abstractions;
using TradingApp.Application.Common;
using TradingApp.Application.Features.Analysis.Common;
using TradingApp.Application.Features.Analysis.Dtos;
using TradingApp.Domain.Enums;

namespace TradingApp.Application.Features.Analysis.Screener;

public sealed class ScreenerQueryHandler
    : IRequestHandler<ScreenerQuery, ScreenerResultDto>
{
    private readonly IAppDbContext _db;
    private readonly IMarketDataProvider _provider;
    private readonly IScreenerCache _cache;
    private readonly ILogger<ScreenerQueryHandler> _logger;

    private const int MaxParallelism = 8;

    public ScreenerQueryHandler(
        IAppDbContext db,
        IMarketDataProvider provider,
        IScreenerCache cache,
        ILogger<ScreenerQueryHandler> logger)
    {
        _db = db;
        _provider = provider;
        _cache = cache;
        _logger = logger;
    }

    public async Task<ScreenerResultDto> Handle(ScreenerQuery request, CancellationToken ct)
    {
        var interval = request.Interval.ToUpperInvariant() switch
        {
            "1W" => "1W",
            "1MO" or "1M" => "1MO",
            _ => "1D"
        };

        var key = BuildCacheKey(request, interval);
        var cached = await _cache.GetAsync(key, ct);
        if (cached is not null)
        {
            _logger.LogInformation("Screener cache HIT: {Key}", key);
            return cached;
        }

        _logger.LogInformation("Screener cache MISS: {Key}", key);

        var query = _db.Assets.AsNoTracking().Where(a => a.IsActive);

        if (!string.IsNullOrWhiteSpace(request.AssetType)
            && Enum.TryParse<AssetType>(request.AssetType, true, out var type))
        {
            query = query.Where(a => a.AssetType == type);
        }

        var assets = await query.ToListAsync(ct);

        var semaphore = new SemaphoreSlim(MaxParallelism);
        var results = new ConcurrentBag<AnalysisScanItemDto>();

        var tasks = assets.Select(async asset =>
        {
            await semaphore.WaitAsync(ct);
            try
            {
                var candles = await _provider.GetCandlesAsync(asset.Symbol, interval, 200, ct);
                if (candles.Count < 35) return;

                var quotes = candles.Select(c => new Quote
                {
                    Date = c.OpenTime.UtcDateTime,
                    Open = c.Open,
                    High = c.High,
                    Low = c.Low,
                    Close = c.Close,
                    Volume = c.Volume
                }).ToList();

                var (score, category, _) = CeilingScoreCalculator.Compute(quotes);
                var rsi = quotes.GetRsi(14).LastOrDefault()?.Rsi;
                var avgVol = quotes.TakeLast(20).Average(q => q.Volume);
                var volRatio = avgVol > 0m ? quotes[^1].Volume / avgVol : 1m;

                var changePct = asset.PreviousClose == 0m ? 0m
                    : Math.Round((asset.CurrentPrice - asset.PreviousClose) / asset.PreviousClose * 100m, 4);

                results.Add(new AnalysisScanItemDto(
                    asset.Id, asset.Symbol, asset.Name, asset.AssetType.ToString(),
                    asset.CurrentPrice, changePct,
                    rsi.HasValue ? Math.Round((decimal)rsi.Value, 2) : null,
                    Math.Round(volRatio, 2),
                    score, category));
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Screener skip {Symbol}", asset.Symbol);
            }
            finally
            {
                semaphore.Release();
            }
        }).ToList();

        await Task.WhenAll(tasks);

        IEnumerable<AnalysisScanItemDto> filtered = results;

        if (request.MinScore.HasValue)
            filtered = filtered.Where(x => x.CeilingScore >= request.MinScore.Value);

        if (request.MaxScore.HasValue)
            filtered = filtered.Where(x => x.CeilingScore <= request.MaxScore.Value);

        if (request.MinRsi.HasValue)
            filtered = filtered.Where(x => x.Rsi14.HasValue && x.Rsi14.Value >= request.MinRsi.Value);

        if (request.MaxRsi.HasValue)
            filtered = filtered.Where(x => x.Rsi14.HasValue && x.Rsi14.Value <= request.MaxRsi.Value);

        if (!string.IsNullOrWhiteSpace(request.Category))
            filtered = filtered.Where(x =>
                string.Equals(x.CeilingCategory, request.Category, StringComparison.OrdinalIgnoreCase));

        var final = filtered
            .OrderByDescending(x => x.CeilingScore)
            .ThenByDescending(x => x.ChangePercent)
            .Take(Math.Clamp(request.Limit, 1, 1000))
            .ToList();

        var result = new ScreenerResultDto(
            assets.Count,
            final.Count,
            interval,
            final,
            DateTimeOffset.UtcNow);

        await _cache.SetAsync(key, result, CacheTtl.ScreenerResult, ct);

        return result;
    }

    private static string BuildCacheKey(ScreenerQuery request, string interval)
    {
        var parts = new List<string>
        {
            request.AssetType ?? "ALL",
            interval,
            request.MinScore?.ToString() ?? "-",
            request.MaxScore?.ToString() ?? "-",
            request.MinRsi?.ToString() ?? "-",
            request.MaxRsi?.ToString() ?? "-",
            request.Category ?? "-",
            request.Limit.ToString()
        };
        return string.Join(":", parts);
    }
}