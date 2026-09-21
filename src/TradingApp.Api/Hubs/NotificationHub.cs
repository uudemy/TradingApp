using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace TradingApp.Api.Hubs;

/// <summary>
/// Kullanıcı bazlı canlı bildirim hub'ı.
/// JWT ile auth zorunlu. Sunucu, Clients.User(userId) ile sadece ilgili
/// kullanıcıya bildirim gönderir.
/// </summary>
[Authorize]
public sealed class NotificationHub : Hub<Application.Abstractions.INotificationHubClient>
{
    private readonly ILogger<NotificationHub> _logger;

    public NotificationHub(ILogger<NotificationHub> logger) => _logger = logger;

    public override async Task OnConnectedAsync()
    {
        _logger.LogInformation("Notification client connected: {ConnectionId} User={User}",
            Context.ConnectionId, Context.UserIdentifier);
        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        _logger.LogInformation("Notification client disconnected: {ConnectionId} User={User}",
            Context.ConnectionId, Context.UserIdentifier);
        await base.OnDisconnectedAsync(exception);
    }
}