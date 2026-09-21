namespace TradingApp.Application.Features.Notifications.Dtos;

public sealed record NotificationDto(
    Guid Id,
    string Title,
    string Message,
    string Type,
    bool IsRead,
    DateTimeOffset CreatedAt);