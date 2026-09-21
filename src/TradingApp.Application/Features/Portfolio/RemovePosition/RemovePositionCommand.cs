using MediatR;

namespace TradingApp.Application.Features.Portfolio.RemovePosition;

public sealed record RemovePositionCommand(Guid PositionId) : IRequest;