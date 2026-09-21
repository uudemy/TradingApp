using Skender.Stock.Indicators;
using TradingApp.Application.Features.Analysis.Dtos;

namespace TradingApp.Application.Features.Analysis.Common;

public static class CeilingScoreCalculator
{
    private static decimal D(double v) => (decimal)v;
    private static decimal? D(double? v) => v.HasValue ? (decimal)v.Value : null;

    public static (int score, string category, List<SignalDto> signals) Compute(
        IReadOnlyList<Quote> quotes)
    {
        if (quotes.Count < 20)
            return (50, "Nötr", new List<SignalDto>
            {
                new("InsufficientData", quotes.Count, "Yetersiz veri", 0, 0)
            });

        var signals = new List<SignalDto>();

        // RSI — double?
        var rsi = quotes.GetRsi(14).LastOrDefault()?.Rsi;
        signals.Add(BuildRsiSignal(rsi));

        // MACD — double?
        var macd = quotes.GetMacd(12, 26, 9).LastOrDefault();
        signals.Add(BuildMacdSignal(macd?.Macd, macd?.Signal, macd?.Histogram));

        // SMA — double?
        var ma20 = quotes.GetSma(20).LastOrDefault()?.Sma;
        var ma50 = quotes.GetSma(50).LastOrDefault()?.Sma;
        var lastClose = quotes[^1].Close;   // decimal
        signals.Add(BuildMaTrendSignal(lastClose, ma20, ma50));

        // Bollinger — double?
        var bb = quotes.GetBollingerBands(20, 2).LastOrDefault();
        signals.Add(BuildBollingerSignal(lastClose, bb?.LowerBand, bb?.UpperBand));

        // Hacim — Quote decimal
        var avgVolume20 = quotes.TakeLast(20).Average(q => q.Volume);
        var volumeRatio = avgVolume20 > 0m ? quotes[^1].Volume / avgVolume20 : 1m;
        signals.Add(BuildVolumeSignal(volumeRatio));

        // Momentum — Quote decimal
        var last5 = quotes.TakeLast(5).ToList();
        var firstClose = last5[0].Close;
        var momentum = firstClose == 0m ? 0m : (lastClose - firstClose) / firstClose * 100m;
        signals.Add(BuildMomentumSignal(momentum));

        int totalWeighted = signals.Sum(s => s.Score * s.Weight);
        int normalized = (int)Math.Round((totalWeighted + 200m) / 400m * 100m);
        normalized = Math.Clamp(normalized, 0, 100);

        var category = normalized switch
        {
            >= 80 => "Güçlü Yükseliş",
            >= 60 => "Yükseliş",
            >= 40 => "Nötr",
            >= 20 => "Düşüş",
            _     => "Güçlü Düşüş"
        };

        return (normalized, category, signals);
    }

    private static SignalDto BuildRsiSignal(double? rsi)
    {
        if (rsi is null) return new SignalDto("RSI(14)", 0m, "Hesaplanamadı", 0, 20);
        var v = rsi.Value;
        var (score, interp) = v switch
        {
            < 30d => (+2, "Aşırı satım — dönüş potansiyeli"),
            < 45d => (+1, "Zayıf — toparlanma olabilir"),
            < 55d => ( 0, "Nötr"),
            < 70d => (-1, "Güçlü — dikkatli ol"),
            _     => (-2, "Aşırı alım — düzeltme riski")
        };
        return new SignalDto("RSI(14)", Math.Round(D(v), 2), interp, score, 20);
    }

    private static SignalDto BuildMacdSignal(double? macd, double? signal, double? hist)
    {
        if (macd is null || signal is null || hist is null)
            return new SignalDto("MACD", 0m, "Hesaplanamadı", 0, 20);

        var h = hist.Value;
        var (score, interp) = h switch
        {
            > 0d when macd > signal => (+2, "Pozitif ve yükselişte"),
            > 0d                    => (+1, "Pozitif"),
            < 0d when macd < signal => (-2, "Negatif ve düşüşte"),
            < 0d                    => (-1, "Negatif"),
            _                       => ( 0, "Nötr")
        };
        return new SignalDto("MACD", Math.Round(D(h), 4), interp, score, 20);
    }

    private static SignalDto BuildMaTrendSignal(decimal close, double? ma20, double? ma50)
    {
        if (ma20 is null || ma50 is null)
            return new SignalDto("MA Trend", 0m, "Hesaplanamadı", 0, 15);

        var aboveMa20 = close > D(ma20.Value);
        var aboveMa50 = close > D(ma50.Value);
        var golden = ma20 > ma50;

        var score = (aboveMa20 ? 1 : -1) + (aboveMa50 ? 1 : -1) + (golden ? 0 : -1);
        score = Math.Clamp(score, -2, 2);

        var interp = (aboveMa20, aboveMa50, golden) switch
        {
            (true,  true,  true)  => "Tüm ortalamaların üzerinde, trend güçlü",
            (true,  true,  false) => "Fiyat yukarıda ama MA20 < MA50",
            (true,  false, _)     => "Kısa vade pozitif, uzun vade zayıf",
            (false, true,  _)     => "Kısa vade zayıf, uzun vade destekliyor",
            _                     => "Ortalamaların altında, zayıf trend"
        };
        return new SignalDto("MA Trend", Math.Round(close, 4), interp, score, 15);
    }

    private static SignalDto BuildBollingerSignal(decimal close, double? lower, double? upper)
    {
        if (lower is null || upper is null)
            return new SignalDto("Bollinger", 0m, "Hesaplanamadı", 0, 15);

        var l = D(lower.Value);
        var u = D(upper.Value);
        var range = u - l;
        if (range == 0m)
            return new SignalDto("Bollinger", 0m, "Bant dar", 0, 15);

        var pos = (close - l) / range;

        var (score, interp) = pos switch
        {
            < 0.10m => (+2, "Alt banda yakın — dönüş potansiyeli"),
            < 0.30m => (+1, "Alt bölgede"),
            < 0.70m => ( 0, "Orta bantta"),
            < 0.90m => (-1, "Üst bölgede"),
            _       => (-2, "Üst banda yakın — düzeltme riski")
        };
        return new SignalDto("Bollinger", Math.Round(pos * 100m, 1), interp, score, 15);
    }

    private static SignalDto BuildVolumeSignal(decimal ratio)
    {
        var (score, interp) = ratio switch
        {
            >= 2.0m => (+2, "Hacim patlaması"),
            >= 1.3m => (+1, "Ortalamanın üzerinde hacim"),
            >= 0.7m => ( 0, "Normal hacim"),
            _       => (-1, "Düşük hacim")
        };
        return new SignalDto("Volume Ratio", Math.Round(ratio, 2), interp, score, 15);
    }

    private static SignalDto BuildMomentumSignal(decimal pct5)
    {
        var (score, interp) = pct5 switch
        {
            >=  5m => (+2, "Son 5 mumda güçlü yükseliş"),
            >=  2m => (+1, "Yükseliş momentumu"),
            >= -2m => ( 0, "Yatay"),
            >= -5m => (-1, "Düşüş momentumu"),
            _      => (-2, "Son 5 mumda sert düşüş")
        };
        return new SignalDto("Momentum(5)", Math.Round(pct5, 2), interp, score, 15);
    }
}