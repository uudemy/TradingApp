using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TradingApp.Api.Common;
using TradingApp.Application.Abstractions;

namespace TradingApp.Api.Controllers;

[ApiController]
[Route("api/admin")]
[Authorize]
public sealed class AdminController : ControllerBase
{
    private readonly IBistAssetSyncService _syncService;
    private readonly ILogger<AdminController> _logger;

    public AdminController(IBistAssetSyncService syncService, ILogger<AdminController> logger)
    {
        _syncService = syncService;
        _logger = logger;
    }

    /// <summary>
    /// BIST Data Service'ten tüm hisseleri ana DB'ye aktarır.
    /// İlk çağrıda ~600 hisse eklenir, sonraki çağrılar fiyat günceller.
    /// Bu işlem 5-10 saniye sürebilir.
    /// </summary>
    [HttpPost("sync-bist-assets")]
    [ProducesResponseType(typeof(ApiResponse<BistSyncResult>), 200)]
    public async Task<IActionResult> SyncBistAssets(CancellationToken ct)
    {
        _logger.LogInformation("Manual BIST sync triggered by user {UserId}",
            User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value);

        var result = await _syncService.SyncAsync(ct);
        return Ok(ApiResponse<BistSyncResult>.Ok(result,
            $"BIST sync complete. Added: {result.Added}, Updated: {result.Updated}, Total: {result.Total}."));
    }
}