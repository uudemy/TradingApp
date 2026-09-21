using MediatR;
using Microsoft.EntityFrameworkCore;
using TradingApp.Application.Abstractions;
using TradingApp.Domain.Common;

namespace TradingApp.Application.Features.Notifications.MarkNotificationRead;

public sealed class MarkNotificationReadCommandHandler : IRequestHandler<MarkNotificationReadCommand>
{
    private readonly IAppDbContext _db;
    private readonly ICurrentUserService _currentUser;

    public MarkNotificationReadCommandHandler(IAppDbContext db, ICurrentUserService currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task Handle(MarkNotificationReadCommand request, CancellationToken ct)
    {
        var userId = _currentUser.UserId
            ?? throw new DomainException("unauthorized", "Authentication required.");

        var notification = await _db.Notifications
            .FirstOrDefaultAsync(n => n.Id == request.NotificationId, ct)
            ?? throw new DomainException("notification_not_found", "Notification not found.");

        if (notification.UserId != userId)
            throw new DomainException("forbidden", "You can only mark your own notifications.");

        if (!notification.IsRead)
        {
            notification.MarkAsRead();
            await _db.SaveChangesAsync(ct);
        }
    }
}