namespace TradingApp.Application.Features.Portfolio.Dtos;

public sealed record PositionDto(
    Guid AssetId,
    string Symbol,
    string Name,
    decimal Quantity,
    decimal LockedQuantity,
    decimal AvailableQuantity,
    decimal AverageCost,
    decimal CurrentPrice,
    decimal MarketValue,
    decimal UnrealizedPnl,
    decimal UnrealizedPnlPercent,
    decimal RealizedPnl);

public sealed record PortfolioSummaryDto(
    decimal TotalMarketValue,
    decimal TotalCost,
    decimal TotalUnrealizedPnl,
    decimal TotalUnrealizedPnlPercent,
    IReadOnlyCollection<PositionDto> Positions);