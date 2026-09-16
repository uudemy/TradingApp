using TradingApp.Domain.Common;

namespace TradingApp.Domain.ValueObjects;

/// <summary>
/// Para birimi + tutar. Finansal hesaplarda asla float/double kullanılmaz.
/// Amount: 18 basamak, 8 ondalık (kripto için yeterli).
/// </summary>
public sealed class Money : ValueObject
{
    public decimal Amount { get; }
    public string Currency { get; }

    private Money() { Currency = default!; } // EF

    public Money(decimal amount, string currency)
    {
        if (string.IsNullOrWhiteSpace(currency) || currency.Length is < 3 or > 10)
            throw new DomainException("invalid_currency",
                "Currency must be 3-10 characters (ISO-4217 or crypto symbol like USDT).");

        Amount = amount;
        Currency = currency.ToUpperInvariant();
    }

    public static Money Zero(string currency) => new(0m, currency);

    public Money Add(Money other)
    {
        EnsureSameCurrency(other);
        return new Money(Amount + other.Amount, Currency);
    }

    public Money Subtract(Money other)
    {
        EnsureSameCurrency(other);
        return new Money(Amount - other.Amount, Currency);
    }

    public Money Multiply(decimal factor) => new(Amount * factor, Currency);

    public bool IsNegative() => Amount < 0;
    public bool IsZero() => Amount == 0m;

    private void EnsureSameCurrency(Money other)
    {
        if (!string.Equals(Currency, other.Currency, StringComparison.OrdinalIgnoreCase))
            throw new DomainException("currency_mismatch",
                $"Cannot operate on different currencies: {Currency} vs {other.Currency}.");
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Amount;
        yield return Currency;
    }

    public override string ToString() => $"{Amount} {Currency}";
}