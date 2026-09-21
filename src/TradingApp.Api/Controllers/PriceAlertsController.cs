using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TradingApp.Api.Common;
using TradingApp.Application.Features.PriceAlerts.CreatePriceAlert;
using TradingApp.Application.Features.PriceAlerts.DeletePriceAlert;
using TradingApp.Application.Features.PriceAlerts.Dtos;
using TradingApp.Application.Features.PriceAlerts.GetPriceAlerts;

namespace TradingApp.Api.Controllers;

[ApiController]
[Route("api/price-alerts")]
[Authorize]
public sealed class PriceAlertsController : ControllerBase
{
    private readonly ISender _sender;
    public PriceAlertsController(ISender sender) => _sender = sender;

    /// <summary>Kullanıcının fiyat alarmlarını listeler.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyCollection<PriceAlertDto>>), 200)]
    public async Task<IActionResult> GetAll(
        [FromQuery] bool? activeOnly,
        CancellationToken ct)
        => Ok(ApiResponse<IReadOnlyCollection<PriceAlertDto>>.Ok(
            await _sender.Send(new GetPriceAlertsQuery(activeOnly), ct)));

    /// <summary>Yeni fiyat alarmı oluşturur.</summary>
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<PriceAlertDto>), 200)]
    public async Task<IActionResult> Create([FromBody] CreatePriceAlertCommand cmd, CancellationToken ct)
        => Ok(ApiResponse<PriceAlertDto>.Ok(await _sender.Send(cmd, ct)));

    /// <summary>Fiyat alarmını siler.</summary>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        await _sender.Send(new DeletePriceAlertCommand(id), ct);
        return NoContent();
    }
}