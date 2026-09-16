using TradingApp.Domain.Common;

namespace TradingApp.Domain.ValueObjects;

public sealed class Symbol : ValueObject
{
    public string Value { get; } = default!;

    private Symbol() { } // EF

    public Symbol(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new DomainException("invalid_symbol", "Symbol cannot be empty.");

        value = value.Trim().ToUpperInvariant();

        if (value.Length > 20)
            throw new DomainException("invalid_symbol", "Symbol too long (max 20).");

        Value = value;
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
    }

    public override string ToString() => Value;
}