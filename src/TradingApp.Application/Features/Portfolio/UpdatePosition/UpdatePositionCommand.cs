using MediatR;

namespace TradingApp.Application.Features.Portfolio.UpdatePosition;

public sealed record UpdatePositionCommand(
    Guid PositionId,
    decimal Quantity,
    decimal AverageCost,
    DateTimeOffset? PurchaseDate = null,
    string? Notes = null) : IRequest;