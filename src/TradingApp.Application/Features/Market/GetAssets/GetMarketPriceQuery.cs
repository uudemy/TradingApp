using MediatR;
using TradingApp.Application.Features.Market.Dtos;

namespace TradingApp.Application.Features.Market.GetMarketPrice;

public sealed record GetMarketPriceQuery(string Symbol) : IRequest<MarketPriceDto>;