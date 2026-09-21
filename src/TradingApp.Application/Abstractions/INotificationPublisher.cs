namespace TradingApp.Application.Abstractions;

public sealed record NotificationPayload(
    Guid Id,
    string Title,
    string Message,
    string Type,
    DateTimeOffset CreatedAt);

public interface INotificationPublisher
{
    /// <summary>Belirli bir kullanıcıya SignalR üzerinden canlı bildirim gönderir.</summary>
    Task PublishAsync(Guid userId, NotificationPayload payload, CancellationToken ct = default);
}