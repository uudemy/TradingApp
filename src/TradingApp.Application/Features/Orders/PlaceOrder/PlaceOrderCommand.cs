using MediatR;
using TradingApp.Application.Features.Orders.Dtos;
using TradingApp.Domain.Enums;

namespace TradingApp.Application.Features.Orders.PlaceOrder;

public sealed record PlaceOrderCommand(
    string Symbol,
    OrderSide Side,
    OrderType OrderType,
    decimal? Price,
    decimal Quantity) : IRequest<OrderDto>;