using TradingApp.Domain.Common;

namespace TradingApp.Domain.Entities;

public sealed class AuditLog : BaseEntity
{
    public Guid? UserId { get; private set; }
    public string Action { get; private set; } = default!;
    public string? EntityType { get; private set; }
    public string? EntityId { get; private set; }
    public string? Metadata { get; private set; }   // JSON
    public string? IpAddress { get; private set; }

    private AuditLog() { } // EF

    public AuditLog(Guid? userId, string action, string? entityType = null,
                    string? entityId = null, string? metadata = null, string? ip = null)
    {
        UserId = userId;
        Action = action;
        EntityType = entityType;
        EntityId = entityId;
        Metadata = metadata;
        IpAddress = ip;
    }
}