using TradingApp.Domain.Common;

namespace TradingApp.Domain.Entities;

public sealed class User : BaseEntity, IAggregateRoot
{
    public string Email { get; private set; } = default!;
    public string PasswordHash { get; private set; } = default!;
    public string FirstName { get; private set; } = default!;
    public string LastName { get; private set; } = default!;
    public bool IsActive { get; private set; } = true;
    public bool IsEmailVerified { get; private set; }

    public Guid? RoleId { get; private set; }
    public Role? Role { get; private set; }

    // Navigation
    public ICollection<Wallet> Wallets { get; private set; } = new List<Wallet>();
    public Portfolio? Portfolio { get; private set; }
    public ICollection<Order> Orders { get; private set; } = new List<Order>();
    public ICollection<WatchlistItem> Watchlist { get; private set; } = new List<WatchlistItem>();

    private User() { } // EF

    public User(string email, string passwordHash, string firstName, string lastName)
    {
        if (string.IsNullOrWhiteSpace(email) || !email.Contains('@'))
            throw new DomainException("invalid_email", "Email is invalid.");
        if (string.IsNullOrWhiteSpace(passwordHash))
            throw new DomainException("invalid_password", "Password hash required.");

        Email = email.Trim().ToLowerInvariant();
        PasswordHash = passwordHash;
        FirstName = firstName.Trim();
        LastName = lastName.Trim();
    }

    public void VerifyEmail() { IsEmailVerified = true; Touch(); }
    public void Deactivate() { IsActive = false; Touch(); }
    public void Activate() { IsActive = true; Touch(); }
    public void ChangePassword(string newHash) { PasswordHash = newHash; Touch(); }
    public void AssignRole(Guid roleId)
    {
        RoleId = roleId;
        Touch();
    }
}