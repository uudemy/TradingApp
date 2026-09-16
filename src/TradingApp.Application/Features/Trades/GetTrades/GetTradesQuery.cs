using MediatR;
using TradingApp.Application.Features.Orders.Dtos;

namespace TradingApp.Application.Features.Trades.GetTrades;

public sealed record GetTradesQuery(string? Symbol = null)
    : IRequest<IReadOnlyCollection<TradeDto>>;