namespace TradingApp.Domain.Common;

/// <summary>
/// Tüm domain entity'lerinin ortak temeli.
/// Domain katmanı hiçbir EF Core / dış pakete bağımlı DEĞİLDİR.
/// </summary>
public abstract class BaseEntity
{
    public Guid Id { get; protected set; } = Guid.NewGuid();
    public DateTimeOffset CreatedAt { get; protected set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset? UpdatedAt { get; protected set; }

    protected void Touch() => UpdatedAt = DateTimeOffset.UtcNow;
}