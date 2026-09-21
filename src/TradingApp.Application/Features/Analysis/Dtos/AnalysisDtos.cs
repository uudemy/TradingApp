namespace TradingApp.Application.Features.Analysis.Dtos;

/// <summary>Tek bir indikatör sinyalinin sonucu.</summary>
public sealed record SignalDto(
    string Name,
    decimal Value,
    string Interpretation,
    int Score,
    int Weight);

/// <summary>Tek bir indikatör noktası (grafik çizimi için).</summary>
public sealed record IndicatorPointDto(DateTimeOffset Time, decimal? Value);

/// <summary>MACD noktası (3 değerli).</summary>
public sealed record MacdPointDto(
    DateTimeOffset Time,
    decimal? Macd,
    decimal? Signal,
    decimal? Histogram);

/// <summary>Tüm indikatör serileri.</summary>
public sealed record IndicatorSeriesDto(
    IReadOnlyCollection<IndicatorPointDto> Ma20,
    IReadOnlyCollection<IndicatorPointDto> Ma50,
    IReadOnlyCollection<IndicatorPointDto> Ma200,
    IReadOnlyCollection<IndicatorPointDto> BollingerUpper,
    IReadOnlyCollection<IndicatorPointDto> BollingerMiddle,
    IReadOnlyCollection<IndicatorPointDto> BollingerLower,
    IReadOnlyCollection<IndicatorPointDto> Rsi,
    IReadOnlyCollection<MacdPointDto> Macd);

/// <summary>Tek hisse için detaylı analiz.</summary>
public sealed record AnalysisDto(
    Guid AssetId,
    string Symbol,
    string Name,
    string AssetType,
    decimal CurrentPrice,
    decimal ChangePercent,
    decimal? Rsi14,
    decimal? Macd,
    decimal? MacdSignal,
    decimal? MacdHistogram,
    decimal? Ma20,
    decimal? Ma50,
    decimal? Ma200,
    decimal? BollingerUpper,
    decimal? BollingerLower,
    decimal? BollingerMiddle,
    decimal? Atr14,
    decimal? AvgVolume20,
    decimal? VolumeRatio,
    int CeilingScore,
    string CeilingCategory,
    IReadOnlyCollection<SignalDto> Signals,
    DateTimeOffset AnalyzedAt,
    IndicatorSeriesDto? Series = null);

/// <summary>Scan sonucu özet satırı.</summary>
public sealed record AnalysisScanItemDto(
    Guid AssetId,
    string Symbol,
    string Name,
    string AssetType,
    decimal CurrentPrice,
    decimal ChangePercent,
    decimal? Rsi14,
    decimal? VolumeRatio,
    int CeilingScore,
    string CeilingCategory);

/// <summary>Scan sonucu (top + bottom).</summary>
public sealed record AnalysisScanDto(
    int TotalScanned,
    int StrongBullish,
    int Bullish,
    int Neutral,
    int Bearish,
    int StrongBearish,
    IReadOnlyCollection<AnalysisScanItemDto> Top,
    IReadOnlyCollection<AnalysisScanItemDto> Bottom,
    DateTimeOffset ScannedAt);

/// <summary>Screener sonucu — filtrelenmiş tam liste.</summary>
public sealed record ScreenerResultDto(
    int TotalScanned,
    int TotalMatched,
    string Interval,
    IReadOnlyCollection<AnalysisScanItemDto> Items,
    DateTimeOffset ScannedAt);