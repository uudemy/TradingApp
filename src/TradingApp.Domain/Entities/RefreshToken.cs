using TradingApp.Domain.Common;

namespace TradingApp.Domain.Entities;

/// <summary>
/// Refresh token'ın KENDİSİ değil, HASH'i saklanır. Plain token yalnızca client'a döner.
/// </summary>
public sealed class RefreshToken : BaseEntity
{
    public Guid UserId { get; private set; }
    public string TokenHash { get; private set; } = default!;
    public DateTimeOffset ExpiresAt { get; private set; }
    public DateTimeOffset? RevokedAt { get; private set; }
    public string? ReplacedByTokenHash { get; private set; }
    public string? CreatedByIp { get; private set; }
    public string? RevokedByIp { get; private set; }

    public bool IsActive => RevokedAt is null && ExpiresAt > DateTimeOffset.UtcNow;
    public bool IsExpired => ExpiresAt <= DateTimeOffset.UtcNow;

    private RefreshToken() { } // EF

    public RefreshToken(
        Guid userId,
        string tokenHash,
        DateTimeOffset expiresAt,
        string? createdByIp = null)
    {
        if (string.IsNullOrWhiteSpace(tokenHash))
            throw new DomainException("invalid_token", "Token hash required.");
        if (expiresAt <= DateTimeOffset.UtcNow)
            throw new DomainException("invalid_expiry", "Expiry must be in the future.");

        UserId = userId;
        TokenHash = tokenHash;
        ExpiresAt = expiresAt;
        CreatedByIp = createdByIp;
    }

    public void Revoke(string? replacedByTokenHash = null, string? revokedByIp = null)
    {
        if (RevokedAt is not null)
            throw new DomainException("already_revoked", "Token already revoked.");

        RevokedAt = DateTimeOffset.UtcNow;
        ReplacedByTokenHash = replacedByTokenHash;
        RevokedByIp = revokedByIp;
        Touch();
    }
}