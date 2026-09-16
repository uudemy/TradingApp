using TradingApp.Domain.Common;
using TradingApp.Domain.Enums;

namespace TradingApp.Domain.Entities;

public sealed class Notification : BaseEntity
{
    public Guid UserId { get; private set; }
    public string Title { get; private set; } = default!;
    public string Message { get; private set; } = default!;
    public NotificationType Type { get; private set; }
    public bool IsRead { get; private set; }

    private Notification() { } // EF

    public Notification(Guid userId, string title, string message, NotificationType type)
    {
        UserId = userId;
        Title = title;
        Message = message;
        Type = type;
    }

    public void MarkAsRead() { IsRead = true; Touch(); }
}