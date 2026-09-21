using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TradingApp.Api.Common;
using TradingApp.Application.Features.Analysis.AnalyzeSymbol;
using TradingApp.Application.Features.Analysis.Dtos;
using TradingApp.Application.Features.Analysis.Scan;

namespace TradingApp.Api.Controllers;

[ApiController]
[Route("api/analysis")]
[AllowAnonymous]
public sealed class AnalysisController : ControllerBase
{
    private readonly ISender _sender;
    public AnalysisController(ISender sender) => _sender = sender;

    /// <summary>
    /// Tek hisse için detaylı teknik analiz + tavan potansiyeli skoru.
    /// interval: 1D, 1W, 1h, 4h. limit: 50..500.
    /// </summary>
    [HttpGet("{symbol}")]
    [ProducesResponseType(typeof(ApiResponse<AnalysisDto>), 200)]
    public async Task<IActionResult> Analyze(
        string symbol,
        [FromQuery] string interval = "1D",
        [FromQuery] int limit = 200,
        CancellationToken ct = default)
        => Ok(ApiResponse<AnalysisDto>.Ok(
            await _sender.Send(new AnalyzeSymbolQuery(symbol, interval, limit), ct)));

    /// <summary>
    /// Tüm aktif hisseleri tarar, tavan potansiyeli skoruna göre sıralar.
    /// assetType: Stock / Crypto / Etf / Forex (opsiyonel filtre).
    /// </summary>
    [HttpGet("scan")]
    [ProducesResponseType(typeof(ApiResponse<AnalysisScanDto>), 200)]
    public async Task<IActionResult> Scan(
        [FromQuery] string? assetType = null,
        [FromQuery] int topCount = 10,
        CancellationToken ct = default)
        => Ok(ApiResponse<AnalysisScanDto>.Ok(
            await _sender.Send(new ScanAnalysisQuery(assetType, Math.Clamp(topCount, 1, 50)), ct)));
}