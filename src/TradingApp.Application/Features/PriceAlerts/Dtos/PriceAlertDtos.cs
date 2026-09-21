namespace TradingApp.Application.Features.PriceAlerts.Dtos;

public sealed record PriceAlertDto(
    Guid Id,
    Guid AssetId,
    string Symbol,
    string Condition,
    decimal TargetPrice,
    bool IsActive,
    DateTimeOffset? TriggeredAt,
    DateTimeOffset CreatedAt);