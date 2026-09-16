using TradingApp.Domain.Common;
using TradingApp.Domain.ValueObjects;

namespace TradingApp.Domain.Entities;

/// <summary>
/// Kullanıcının tek bir currency cinsinden cüzdanı.
/// Available + Locked. Locked, açık emirler için ayrılmış bakiyedir.
/// </summary>
public sealed class Wallet : BaseEntity, IAggregateRoot
{
    public Guid UserId { get; private set; }
    public string Currency { get; private set; } = default!;
    public decimal AvailableBalance { get; private set; }
    public decimal LockedBalance { get; private set; }

    /// <summary>Optimistic concurrency için.</summary>
    public uint RowVersion { get; private set; }
    private Wallet() { } // EF

    public Wallet(Guid userId, string currency, decimal initialBalance = 0m)
    {
        if (initialBalance < 0)
            throw new DomainException("invalid_balance", "Initial balance cannot be negative.");

        UserId = userId;
        Currency = currency.ToUpperInvariant();
        AvailableBalance = initialBalance;
        LockedBalance = 0m;
    }

    public decimal TotalBalance => AvailableBalance + LockedBalance;

    /// <summary>Para yatırma (demo seed veya admin işlemi).</summary>
    public void Deposit(decimal amount)
    {
        if (amount <= 0) throw new DomainException("invalid_amount", "Amount must be positive.");
        AvailableBalance += amount;
        Touch();
    }

    /// <summary>Emir verildiğinde bakiyeyi kilitle.</summary>
    public void Lock(decimal amount)
    {
        if (amount <= 0) throw new DomainException("invalid_amount", "Lock amount must be positive.");
        if (AvailableBalance < amount)
            throw new DomainException("insufficient_balance",
                $"Insufficient {Currency} balance. Required: {amount}, Available: {AvailableBalance}.");

        AvailableBalance -= amount;
        LockedBalance += amount;
        Touch();
    }

    /// <summary>Emir iptal edildiğinde kilidi çöz.</summary>
    public void Unlock(decimal amount)
    {
        if (amount <= 0) throw new DomainException("invalid_amount", "Unlock amount must be positive.");
        if (LockedBalance < amount)
            throw new DomainException("invalid_lock_state",
                $"Unlock amount exceeds locked balance. Required: {amount}, Locked: {LockedBalance}.");

        LockedBalance -= amount;
        AvailableBalance += amount;
        Touch();
    }

    /// <summary>Trade gerçekleştiğinde kilitli bakiyeyi harcar (BUY tarafı).</summary>
    public void ConsumeLocked(decimal amount)
    {
        if (amount <= 0) throw new DomainException("invalid_amount", "Amount must be positive.");
        if (LockedBalance < amount)
            throw new DomainException("insufficient_locked_balance",
                $"Consume exceeds locked. Required: {amount}, Locked: {LockedBalance}.");

        LockedBalance -= amount;
        Touch();
    }

        /// <summary>Gerçekleşen alımda kullanılabilir bakiyeden düş.</summary>
    public void Withdraw(decimal amount)
    {
        if (amount <= 0) throw new DomainException("invalid_amount", "Amount must be positive.");
        if (AvailableBalance < amount)
            throw new DomainException("insufficient_balance",
                $"Insufficient {Currency}. Required: {amount}, Available: {AvailableBalance}.");

        AvailableBalance -= amount;
        Touch();
    }

    /// <summary>Trade gerçekleştiğinde alıcıya/satıcıya bakiye ekle (SELL tarafı veya BUY fill).</summary>
    public void Credit(decimal amount)
    {
        if (amount <= 0) throw new DomainException("invalid_amount", "Amount must be positive.");
        AvailableBalance += amount;
        Touch();
    }
}