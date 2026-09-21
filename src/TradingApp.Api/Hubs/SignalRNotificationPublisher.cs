using Microsoft.AspNetCore.SignalR;
using TradingApp.Application.Abstractions;

namespace TradingApp.Api.Hubs;

public sealed class SignalRNotificationPublisher : INotificationPublisher
{
    private readonly IHubContext<NotificationHub, INotificationHubClient> _hub;
    private readonly ILogger<SignalRNotificationPublisher> _logger;

    public SignalRNotificationPublisher(
        IHubContext<NotificationHub, INotificationHubClient> hub,
        ILogger<SignalRNotificationPublisher> logger)
    {
        _hub = hub;
        _logger = logger;
    }

    public async Task PublishAsync(Guid userId, NotificationPayload payload, CancellationToken ct = default)
    {
        try
        {
            await _hub.Clients.User(userId.ToString()).NotificationReceived(payload);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Notification publish failed for user {UserId}", userId);
        }
    }
}