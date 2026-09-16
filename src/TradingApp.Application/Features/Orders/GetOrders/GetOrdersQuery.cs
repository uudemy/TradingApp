using MediatR;
using TradingApp.Application.Features.Orders.Dtos;

namespace TradingApp.Application.Features.Orders.GetOrders;

public sealed record GetOrdersQuery(string? Status = null, string? Symbol = null)
    : IRequest<IReadOnlyCollection<OrderDto>>;