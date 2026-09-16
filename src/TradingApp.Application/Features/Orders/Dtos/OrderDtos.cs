namespace TradingApp.Application.Features.Orders.Dtos;

public sealed record OrderDto(
    Guid Id,
    string Symbol,
    string Side,
    string OrderType,
    string Status,
    decimal Price,
    decimal Quantity,
    decimal FilledQuantity,
    decimal RemainingQuantity,
    decimal AverageFillPrice,
    DateTimeOffset CreatedAt);

public sealed record TradeDto(
    Guid Id,
    string Symbol,
    decimal Price,
    decimal Quantity,
    decimal Total,
    Guid BuyOrderId,
    Guid SellOrderId,
    DateTimeOffset CreatedAt);