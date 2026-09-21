using MediatR;

namespace TradingApp.Application.Features.Portfolio.AddManualPosition;

public sealed record AddManualPositionCommand(
    string Symbol,
    decimal Quantity,
    decimal AverageCost,
    DateTimeOffset? PurchaseDate = null,
    string? Notes = null) : IRequest<Guid>;