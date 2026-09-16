using MediatR;
using TradingApp.Application.Features.Portfolio.Dtos;

namespace TradingApp.Application.Features.Portfolio.GetPortfolio;

public sealed record GetPortfolioQuery : IRequest<PortfolioSummaryDto>;