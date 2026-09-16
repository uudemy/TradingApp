using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TradingApp.Api.Common;
using TradingApp.Application.Features.Portfolio.Dtos;
using TradingApp.Application.Features.Portfolio.GetPortfolio;

namespace TradingApp.Api.Controllers;

[ApiController]
[Route("api/portfolio")]
[Authorize]
public sealed class PortfolioController : ControllerBase
{
    private readonly ISender _sender;
    public PortfolioController(ISender sender) => _sender = sender;

    /// <summary>Portföy özeti + tüm pozisyonlar (P/L dahil).</summary>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<PortfolioSummaryDto>), 200)]
    public async Task<IActionResult> Get(CancellationToken ct)
        => Ok(ApiResponse<PortfolioSummaryDto>.Ok(
            await _sender.Send(new GetPortfolioQuery(), ct)));
}