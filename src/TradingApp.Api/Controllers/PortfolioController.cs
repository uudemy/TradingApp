using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TradingApp.Api.Common;
using TradingApp.Application.Features.Portfolio.AddManualPosition;
using TradingApp.Application.Features.Portfolio.Dtos;
using TradingApp.Application.Features.Portfolio.GetPortfolio;
using TradingApp.Application.Features.Portfolio.RemovePosition;
using TradingApp.Application.Features.Portfolio.UpdatePosition;

namespace TradingApp.Api.Controllers;

[ApiController]
[Route("api/portfolio")]
[Authorize]
public sealed class PortfolioController : ControllerBase
{
    private readonly ISender _sender;
    public PortfolioController(ISender sender) => _sender = sender;

    /// <summary>Portföy özeti + tüm pozisyonlar (canlı P/L dahil).</summary>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<PortfolioSummaryDto>), 200)]
    public async Task<IActionResult> Get(CancellationToken ct)
        => Ok(ApiResponse<PortfolioSummaryDto>.Ok(
            await _sender.Send(new GetPortfolioQuery(), ct)));

    /// <summary>Elle pozisyon ekler (mevcut varsa ağırlıklı ortalama ile birleşir).</summary>
    [HttpPost("positions")]
    [ProducesResponseType(typeof(ApiResponse<Guid>), 200)]
    public async Task<IActionResult> AddPosition(
        [FromBody] AddManualPositionCommand cmd, CancellationToken ct)
        => Ok(ApiResponse<Guid>.Ok(await _sender.Send(cmd, ct)));

    /// <summary>Pozisyonu günceller (adet, ortalama maliyet, tarih, not).</summary>
    [HttpPut("positions/{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> UpdatePosition(
        Guid id, [FromBody] UpdatePositionBody body, CancellationToken ct)
    {
        await _sender.Send(new UpdatePositionCommand(
            id, body.Quantity, body.AverageCost, body.PurchaseDate, body.Notes), ct);
        return NoContent();
    }

    /// <summary>Pozisyonu siler.</summary>
    [HttpDelete("positions/{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> RemovePosition(Guid id, CancellationToken ct)
    {
        await _sender.Send(new RemovePositionCommand(id), ct);
        return NoContent();
    }
}

public sealed record UpdatePositionBody(
    decimal Quantity,
    decimal AverageCost,
    DateTimeOffset? PurchaseDate = null,
    string? Notes = null);