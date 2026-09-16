using TradingApp.Domain.Common;
using TradingApp.Domain.Enums;

namespace TradingApp.Domain.Entities;

/// <summary>
/// Cüzdan hareketleri (audit trail). Değiştirilemez — sadece insert.
/// </summary>
public sealed class WalletTransaction : BaseEntity
{
    public Guid WalletId { get; private set; }
    public WalletTransactionType Type { get; private set; }
    public decimal Amount { get; private set; }
    public decimal BalanceAfter { get; private set; }
    public string? Reference { get; private set; }  // OrderId, TradeId vb.
    public string? Description { get; private set; }

    private WalletTransaction() { } // EF

    public WalletTransaction(
        Guid walletId,
        WalletTransactionType type,
        decimal amount,
        decimal balanceAfter,
        string? reference = null,
        string? description = null)
    {
        WalletId = walletId;
        Type = type;
        Amount = amount;
        BalanceAfter = balanceAfter;
        Reference = reference;
        Description = description;
    }
}