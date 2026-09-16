using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TradingApp.Api.Common;
using TradingApp.Application.Features.Auth.Dtos;
using TradingApp.Application.Features.Auth.Login;
using TradingApp.Application.Features.Auth.Logout;
using TradingApp.Application.Features.Auth.Refresh;
using TradingApp.Application.Features.Auth.Register;

namespace TradingApp.Api.Controllers;

[ApiController]
[Route("api/auth")]
[AllowAnonymous]
public sealed class AuthController : ControllerBase
{
    private readonly ISender _sender;
    public AuthController(ISender sender) => _sender = sender;

    [HttpPost("register")]
    [ProducesResponseType(typeof(ApiResponse<AuthResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Register([FromBody] RegisterCommand cmd, CancellationToken ct)
        => Ok(ApiResponse<AuthResponse>.Ok(await _sender.Send(cmd, ct)));

    [HttpPost("login")]
    [ProducesResponseType(typeof(ApiResponse<AuthResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Login([FromBody] LoginCommand cmd, CancellationToken ct)
        => Ok(ApiResponse<AuthResponse>.Ok(await _sender.Send(cmd, ct)));

    [HttpPost("refresh")]
    [ProducesResponseType(typeof(ApiResponse<AuthResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Refresh([FromBody] RefreshTokenCommand cmd, CancellationToken ct)
        => Ok(ApiResponse<AuthResponse>.Ok(await _sender.Send(cmd, ct)));

    [HttpPost("logout")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Logout([FromBody] LogoutCommand cmd, CancellationToken ct)
    {
        await _sender.Send(cmd, ct);
        return NoContent();
    }
}