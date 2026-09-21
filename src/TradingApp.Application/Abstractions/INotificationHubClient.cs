namespace TradingApp.Application.Abstractions;

public interface INotificationHubClient
{
    Task NotificationReceived(NotificationPayload payload);
}