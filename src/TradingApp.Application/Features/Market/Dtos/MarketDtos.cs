namespace TradingApp.Application.Features.Market.Dtos;

public sealed record AssetDto(
    Guid Id,
    string Symbol,
    string Name,
    string AssetType,
    string Currency,
    decimal CurrentPrice,
    decimal PreviousClose,
    decimal DailyVolume,
    bool IsActive,
    decimal ChangePercent);

public sealed record MarketPriceDto(
    string Symbol,
    decimal Price,
    decimal PreviousClose,
    decimal ChangePercent,
    decimal DailyVolume,
    DateTimeOffset TimestampUtc);

public sealed record CandleDto(
    DateTimeOffset OpenTime,
    decimal Open,
    decimal High,
    decimal Low,
    decimal Close,
    decimal Volume);