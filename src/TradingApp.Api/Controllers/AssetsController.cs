using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TradingApp.Api.Common;
using TradingApp.Application.Features.Market.Dtos;
using TradingApp.Application.Features.Market.GetAssetBySymbol;
using TradingApp.Application.Features.Market.GetAssets;

namespace TradingApp.Api.Controllers;

[ApiController]
[Route("api/assets")]
[AllowAnonymous]
public sealed class AssetsController : ControllerBase
{
    private readonly ISender _sender;
    public AssetsController(ISender sender) => _sender = sender;

    /// <summary>Aktif tüm asset'leri listeler (Redis cache'li).</summary>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyCollection<AssetDto>>), 200)]
    public async Task<IActionResult> GetAll(CancellationToken ct)
        => Ok(ApiResponse<IReadOnlyCollection<AssetDto>>.Ok(
            await _sender.Send(new GetAssetsQuery(), ct)));

    /// <summary>Sembol ile tek asset döner.</summary>
    [HttpGet("{symbol}")]
    [ProducesResponseType(typeof(ApiResponse<AssetDto>), 200)]
    [ProducesResponseType(typeof(ApiResponse<AssetDto>), 404)]
    public async Task<IActionResult> GetBySymbol(string symbol, CancellationToken ct)
        => Ok(ApiResponse<AssetDto>.Ok(
            await _sender.Send(new GetAssetBySymbolQuery(symbol), ct)));
}