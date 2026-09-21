using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TradingApp.Api.Common;
using TradingApp.Application.Features.Analysis.AnalyzeSymbol;
using TradingApp.Application.Features.Analysis.Dtos;
using TradingApp.Application.Features.Analysis.Scan;
using TradingApp.Application.Features.Analysis.Screener;

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
    /// interval: 1D, 1W, 1MO, 1h, 4h. limit: 50..500.
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
    /// assetType: Stock / Crypto / Etf / Forex (opsiyonel).
    /// </summary>
    [HttpGet("scan")]
    [ProducesResponseType(typeof(ApiResponse<AnalysisScanDto>), 200)]
    public async Task<IActionResult> Scan(
        [FromQuery] string? assetType = null,
        [FromQuery] int topCount = 10,
        CancellationToken ct = default)
        => Ok(ApiResponse<AnalysisScanDto>.Ok(
            await _sender.Send(new ScanAnalysisQuery(assetType, Math.Clamp(topCount, 1, 50)), ct)));

    /// <summary>
    /// Fırsat tarayıcı — tüm hisseleri skorlar, filtrelere göre sıralı döner.
    /// Örnek: /api/analysis/screener?assetType=Stock&amp;interval=1D&amp;minScore=60
    /// </summary>
    [HttpGet("screener")]
    [ProducesResponseType(typeof(ApiResponse<ScreenerResultDto>), 200)]
    public async Task<IActionResult> Screener(
        [FromQuery] string? assetType = null,
        [FromQuery] string interval = "1D",
        [FromQuery] int? minScore = null,
        [FromQuery] int? maxScore = null,
        [FromQuery] decimal? minRsi = null,
        [FromQuery] decimal? maxRsi = null,
        [FromQuery] string? category = null,
        [FromQuery] int limit = 200,
        CancellationToken ct = default)
        => Ok(ApiResponse<ScreenerResultDto>.Ok(
            await _sender.Send(new ScreenerQuery(
                assetType, interval, minScore, maxScore, minRsi, maxRsi, category, limit), ct)));
}