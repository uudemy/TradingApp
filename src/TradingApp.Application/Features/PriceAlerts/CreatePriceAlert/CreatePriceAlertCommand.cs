using MediatR;
using TradingApp.Application.Features.PriceAlerts.Dtos;
using TradingApp.Domain.Enums;

namespace TradingApp.Application.Features.PriceAlerts.CreatePriceAlert;

public sealed record CreatePriceAlertCommand(
    string Symbol,
    PriceAlertCondition Condition,
    decimal TargetPrice) : IRequest<PriceAlertDto>;