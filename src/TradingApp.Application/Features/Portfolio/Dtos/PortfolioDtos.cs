namespace TradingApp.Application.Features.Portfolio.Dtos;

public sealed record PositionDto(
    Guid Id,
    Guid AssetId,
    string Symbol,
    string Name,
    string Currency,
    decimal Quantity,
    decimal LockedQuantity,
    decimal AvailableQuantity,
    decimal AverageCost,
    decimal CurrentPrice,
    decimal PreviousClose,
    decimal MarketValue,
    decimal CostBasis,
    decimal UnrealizedPnl,
    decimal UnrealizedPnlPercent,
    decimal DailyPnl,
    decimal RealizedPnl,
    DateTimeOffset? PurchaseDate,
    string? Notes);

public sealed record CashBalanceDto(
    string Currency,
    decimal Available,
    decimal Locked,
    decimal Total);

public sealed record PortfolioSummaryDto(
    decimal TotalMarketValue,
    decimal TotalCostBasis,
    decimal TotalUnrealizedPnl,
    decimal TotalUnrealizedPnlPercent,
    decimal TotalDailyPnl,
    IReadOnlyCollection<CashBalanceDto> CashBalances,
    IReadOnlyCollection<PositionDto> Positions);