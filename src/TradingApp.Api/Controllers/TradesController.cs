using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TradingApp.Api.Common;
using TradingApp.Application.Features.Orders.Dtos;
using TradingApp.Application.Features.Trades.GetTrades;

namespace TradingApp.Api.Controllers;

[ApiController]
[Route("api/trades")]
[Authorize]
public sealed class TradesController : ControllerBase
{
    private readonly ISender _sender;
    public TradesController(ISender sender) => _sender = sender;

    /// <summary>Kullanıcının gerçekleşmiş işlemlerini listeler.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyCollection<TradeDto>>), 200)]
    public async Task<IActionResult> GetAll([FromQuery] string? symbol, CancellationToken ct)
        => Ok(ApiResponse<IReadOnlyCollection<TradeDto>>.Ok(
            await _sender.Send(new GetTradesQuery(symbol), ct)));
}