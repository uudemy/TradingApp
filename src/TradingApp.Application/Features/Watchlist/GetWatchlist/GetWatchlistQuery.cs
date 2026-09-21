using MediatR;
using TradingApp.Application.Features.Watchlist.Dtos;

namespace TradingApp.Application.Features.Watchlist.GetWatchlist;

public sealed record GetWatchlistQuery : IRequest<IReadOnlyCollection<WatchlistItemDto>>;