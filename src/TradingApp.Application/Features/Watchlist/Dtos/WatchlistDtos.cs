namespace TradingApp.Application.Features.Watchlist.Dtos;

public sealed record WatchlistItemDto(
    Guid AssetId,
    string Symbol,
    string Name,
    string AssetType,
    string Currency,
    decimal CurrentPrice,
    decimal ChangePercent,
    DateTimeOffset AddedAt);