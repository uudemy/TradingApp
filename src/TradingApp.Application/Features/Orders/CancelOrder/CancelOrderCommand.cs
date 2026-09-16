using MediatR;

namespace TradingApp.Application.Features.Orders.CancelOrder;

public sealed record CancelOrderCommand(Guid OrderId) : IRequest;