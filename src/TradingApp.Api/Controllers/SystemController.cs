using MediatR;
using Microsoft.AspNetCore.Mvc;
using TradingApp.Api.Common;
using TradingApp.Application.System;

namespace TradingApp.Api.Controllers;

[ApiController]
[Route("api/system")]
public sealed class SystemController : ControllerBase
{
    private readonly ISender _sender;

    public SystemController(ISender sender) => _sender = sender;

    /// <summary>Servisin ayakta olduğunu doğrulamak için basit ping.</summary>
    [HttpGet("ping")]
    [ProducesResponseType(typeof(ApiResponse<PingResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Ping(CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new PingQuery(), cancellationToken);
        return Ok(ApiResponse<PingResponse>.Ok(result, "pong"));
    }
}