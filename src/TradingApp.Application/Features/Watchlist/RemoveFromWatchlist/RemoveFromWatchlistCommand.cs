using MediatR;

namespace TradingApp.Application.Features.Watchlist.RemoveFromWatchlist;

public sealed record RemoveFromWatchlistCommand(Guid AssetId) : IRequest;