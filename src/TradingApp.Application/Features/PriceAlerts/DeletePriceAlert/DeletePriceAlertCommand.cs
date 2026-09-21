using MediatR;

namespace TradingApp.Application.Features.PriceAlerts.DeletePriceAlert;

public sealed record DeletePriceAlertCommand(Guid AlertId) : IRequest;