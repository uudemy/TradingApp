namespace TradingApp.Application.Features.Wallets.Dtos;

public sealed record WalletDto(
    Guid Id,
    string Currency,
    decimal AvailableBalance,
    decimal LockedBalance,
    decimal TotalBalance);