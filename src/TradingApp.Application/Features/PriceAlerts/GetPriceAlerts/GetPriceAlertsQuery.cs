using MediatR;
using TradingApp.Application.Features.PriceAlerts.Dtos;

namespace TradingApp.Application.Features.PriceAlerts.GetPriceAlerts;

public sealed record GetPriceAlertsQuery(bool? ActiveOnly = null)
    : IRequest<IReadOnlyCollection<PriceAlertDto>>;