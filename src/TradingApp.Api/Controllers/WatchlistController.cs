using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TradingApp.Api.Common;
using TradingApp.Application.Features.Watchlist.AddToWatchlist;
using TradingApp.Application.Features.Watchlist.Dtos;
using TradingApp.Application.Features.Watchlist.GetWatchlist;
using TradingApp.Application.Features.Watchlist.RemoveFromWatchlist;

namespace TradingApp.Api.Controllers;

[ApiController]
[Route("api/watchlist")]
[Authorize]
public sealed class WatchlistController : ControllerBase
{
    private readonly ISender _sender;
    public WatchlistController(ISender sender) => _sender = sender;

    /// <summary>Kullanıcının favori hisselerini listeler.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyCollection<WatchlistItemDto>>), 200)]
    public async Task<IActionResult> Get(CancellationToken ct)
        => Ok(ApiResponse<IReadOnlyCollection<WatchlistItemDto>>.Ok(
            await _sender.Send(new GetWatchlistQuery(), ct)));

    /// <summary>Favori listesine hisse ekler.</summary>
    [HttpPost("{symbol}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Add(string symbol, CancellationToken ct)
    {
        await _sender.Send(new AddToWatchlistCommand(symbol), ct);
        return NoContent();
    }

    /// <summary>Favori listesinden hisse çıkarır.</summary>
    [HttpDelete("{assetId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Remove(Guid assetId, CancellationToken ct)
    {
        await _sender.Send(new RemoveFromWatchlistCommand(assetId), ct);
        return NoContent();
    }
}