using MediatR;
using Microsoft.EntityFrameworkCore;
using Skender.Stock.Indicators;
using TradingApp.Application.Abstractions;
using TradingApp.Application.Features.Analysis.Common;
using TradingApp.Application.Features.Analysis.Dtos;
using TradingApp.Domain.Common;

namespace TradingApp.Application.Features.Analysis.AnalyzeSymbol;

public sealed class AnalyzeSymbolQueryHandler
    : IRequestHandler<AnalyzeSymbolQuery, AnalysisDto>
{
    private readonly IAppDbContext _db;
    private readonly IMarketDataProvider _provider;

    public AnalyzeSymbolQueryHandler(IAppDbContext db, IMarketDataProvider provider)
    {
        _db = db;
        _provider = provider;
    }

    public async Task<AnalysisDto> Handle(AnalyzeSymbolQuery request, CancellationToken ct)
    {
        var symbol = request.Symbol.Trim().ToUpperInvariant();

        var asset = await _db.Assets.AsNoTracking().FirstOrDefaultAsync(a => a.Symbol == symbol, ct)
            ?? throw new DomainException("asset_not_found", $"Asset '{symbol}' not found.");

        var candles = await _provider.GetCandlesAsync(symbol, request.Interval, request.Limit, ct);
        if (candles.Count < 20)
            throw new DomainException("insufficient_data",
                $"Not enough candle data ({candles.Count}). Need at least 20.");

        // Quote: decimal alanlar (Skender 2.x)
        var quotes = candles
            .Select(c => new Quote
            {
                Date = c.OpenTime.UtcDateTime,
                Open = c.Open,
                High = c.High,
                Low = c.Low,
                Close = c.Close,
                Volume = c.Volume
            })
            .ToList();

        // İndikatörler double? döner
        var rsi = quotes.GetRsi(14).LastOrDefault()?.Rsi;
        var macd = quotes.GetMacd(12, 26, 9).LastOrDefault();
        var ma20 = quotes.GetSma(20).LastOrDefault()?.Sma;
        var ma50 = quotes.GetSma(50).LastOrDefault()?.Sma;
        var ma200 = quotes.Count >= 200 ? quotes.GetSma(200).LastOrDefault()?.Sma : null;
        var bb = quotes.GetBollingerBands(20, 2).LastOrDefault();
        var atr = quotes.Count >= 14 ? quotes.GetAtr(14).LastOrDefault()?.Atr : null;

        var lastVol = quotes[^1].Volume;
        var avgVolume20 = quotes.TakeLast(20).Average(q => q.Volume);
        var volumeRatio = avgVolume20 > 0m ? lastVol / avgVolume20 : 1m;

        var (score, category, signals) = CeilingScoreCalculator.Compute(quotes);

        var changePct = asset.PreviousClose == 0m ? 0m
            : Math.Round((asset.CurrentPrice - asset.PreviousClose) / asset.PreviousClose * 100m, 4);

        return new AnalysisDto(
            asset.Id, asset.Symbol, asset.Name, asset.AssetType.ToString(),
            asset.CurrentPrice, changePct,

            R(rsi),
            R(macd?.Macd),
            R(macd?.Signal),
            R(macd?.Histogram),
            R(ma20),
            R(ma50),
            R(ma200),
            R(bb?.UpperBand),
            R(bb?.LowerBand),
            R(bb?.Sma),
            R(atr),
            Math.Round(avgVolume20, 0),
            Math.Round(volumeRatio, 2),

            score, category, signals,
            DateTimeOffset.UtcNow);
    }

    /// <summary>double? → decimal? (4 basamak yuvarlanmış).</summary>
    private static decimal? R(double? v)
        => v.HasValue ? Math.Round((decimal)v.Value, 4) : null;
}