using MediatR;

namespace TradingApp.Application.Features.Watchlist.AddToWatchlist;

public sealed record AddToWatchlistCommand(string Symbol) : IRequest;