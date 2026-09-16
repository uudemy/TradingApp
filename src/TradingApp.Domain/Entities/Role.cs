using TradingApp.Domain.Common;

namespace TradingApp.Domain.Entities;

public sealed class Role : BaseEntity
{
    public string Name { get; private set; } = default!;
    public string? Description { get; private set; }

    private Role() { } // EF

    public Role(string name, string? description = null)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException("invalid_role", "Role name required.");
        Name = name.Trim();
        Description = description;
    }
}