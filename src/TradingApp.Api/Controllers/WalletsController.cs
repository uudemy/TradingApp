using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TradingApp.Api.Common;
using TradingApp.Application.Features.Wallets.Dtos;
using TradingApp.Application.Features.Wallets.GetWallets;

namespace TradingApp.Api.Controllers;

[ApiController]
[Route("api/wallets")]
[Authorize]
public sealed class WalletsController : ControllerBase
{
    private readonly ISender _sender;
    public WalletsController(ISender sender) => _sender = sender;

    /// <summary>Kullanıcının tüm cüzdanlarını döner (TRY, USD, USDT).</summary>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyCollection<WalletDto>>), 200)]
    public async Task<IActionResult> GetAll(CancellationToken ct)
        => Ok(ApiResponse<IReadOnlyCollection<WalletDto>>.Ok(
            await _sender.Send(new GetWalletsQuery(), ct)));
}