using MediatR;
using TradingApp.Application.Features.Market.Dtos;

namespace TradingApp.Application.Features.Market.GetCandles;

public sealed record GetCandlesQuery(string Symbol, string Interval, int Limit = 200)
    : IRequest<IReadOnlyCollection<CandleDto>>;