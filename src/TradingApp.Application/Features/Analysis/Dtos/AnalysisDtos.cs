namespace TradingApp.Application.Features.Analysis.Dtos;

/// <summary>Tek bir indikatör sinyalinin sonucu.</summary>
public sealed record SignalDto(
    string Name,
    decimal Value,
    string Interpretation,
    int Score,          // -2..+2 arası
    int Weight);        // Toplam skora katkı ağırlığı

/// <summary>Tek bir hisse için detaylı analiz.</summary>
public sealed record AnalysisDto(
    Guid AssetId,
    string Symbol,
    string Name,
    string AssetType,
    decimal CurrentPrice,
    decimal ChangePercent,

    // İndikatör değerleri
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

    // Skor
    int CeilingScore,           // 0..100
    string CeilingCategory,     // "Güçlü Yükseliş" / "Yükseliş" / "Nötr" / "Düşüş" / "Güçlü Düşüş"

    // Sinyaller
    IReadOnlyCollection<SignalDto> Signals,

    DateTimeOffset AnalyzedAt);

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

/// <summary>Scan sonucu (üst grup + özet).</summary>
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