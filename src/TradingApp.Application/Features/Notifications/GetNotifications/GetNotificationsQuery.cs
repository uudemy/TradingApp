using MediatR;
using TradingApp.Application.Features.Notifications.Dtos;

namespace TradingApp.Application.Features.Notifications.GetNotifications;

public sealed record GetNotificationsQuery(bool UnreadOnly = false)
    : IRequest<IReadOnlyCollection<NotificationDto>>;