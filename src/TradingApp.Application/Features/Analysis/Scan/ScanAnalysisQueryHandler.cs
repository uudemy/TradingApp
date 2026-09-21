using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Skender.Stock.Indicators;
using TradingApp.Application.Abstractions;
using TradingApp.Application.Features.Analysis.Common;
using TradingApp.Application.Features.Analysis.Dtos;
using TradingApp.Domain.Enums;

namespace TradingApp.Application.Features.Analysis.Scan;

public sealed class ScanAnalysisQueryHandler
    : IRequestHandler<ScanAnalysisQuery, AnalysisScanDto>
{
    private readonly IAppDbContext _db;
    private readonly IMarketDataProvider _provider;
    private readonly ILogger<ScanAnalysisQueryHandler> _logger;

    public ScanAnalysisQueryHandler(
        IAppDbContext db,
        IMarketDataProvider provider,
        ILogger<ScanAnalysisQueryHandler> logger)
    {
        _db = db;
        _provider = provider;
        _logger = logger;
    }

    public async Task<AnalysisScanDto> Handle(ScanAnalysisQuery request, CancellationToken ct)
    {
        var query = _db.Assets.AsNoTracking().Where(a => a.IsActive);

        if (!string.IsNullOrWhiteSpace(request.AssetType)
            && Enum.TryParse<AssetType>(request.AssetType, true, out var type))
        {
            query = query.Where(a => a.AssetType == type);
        }

        var assets = await query.ToListAsync(ct);
        var results = new List<AnalysisScanItemDto>(assets.Count);

        foreach (var asset in assets)
        {
            try
            {
                var candles = await _provider.GetCandlesAsync(asset.Symbol, "1D", 100, ct);
                if (candles.Count < 20) continue;

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
                _logger.LogWarning(ex, "Scan skip {Symbol}", asset.Symbol);
            }
        }

        var ordered = results.OrderByDescending(r => r.CeilingScore).ToList();

        var top = ordered.Take(request.TopCount).ToList();
        var bottom = ordered.TakeLast(request.TopCount).Reverse().ToList();

        return new AnalysisScanDto(
            results.Count,
            results.Count(r => r.CeilingScore >= 80),
            results.Count(r => r.CeilingScore is >= 60 and < 80),
            results.Count(r => r.CeilingScore is >= 40 and < 60),
            results.Count(r => r.CeilingScore is >= 20 and < 40),
            results.Count(r => r.CeilingScore < 20),
            top, bottom,
            DateTimeOffset.UtcNow);
    }
}