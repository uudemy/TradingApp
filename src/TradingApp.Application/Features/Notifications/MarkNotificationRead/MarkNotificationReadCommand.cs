using MediatR;

namespace TradingApp.Application.Features.Notifications.MarkNotificationRead;

public sealed record MarkNotificationReadCommand(Guid NotificationId) : IRequest;