using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TradingApp.Api.Common;
using TradingApp.Application.Features.Market.Dtos;
using TradingApp.Application.Features.Market.GetCandles;
using TradingApp.Application.Features.Market.GetMarketPrice;

namespace TradingApp.Api.Controllers;

[ApiController]
[Route("api/market")]
[AllowAnonymous]
public sealed class MarketController : ControllerBase
{
    private readonly ISender _sender;
    public MarketController(ISender sender) => _sender = sender;

    /// <summary>Anlık fiyat (Redis cache, ~2 sn TTL).</summary>
    [HttpGet("{symbol}")]
    [ProducesResponseType(typeof(ApiResponse<MarketPriceDto>), 200)]
    public async Task<IActionResult> GetPrice(string symbol, CancellationToken ct)
        => Ok(ApiResponse<MarketPriceDto>.Ok(
            await _sender.Send(new GetMarketPriceQuery(symbol), ct)));

    /// <summary>
    /// Mum (OHLC) verisi. interval: 1s, 1m, 5m, 15m, 1h, 4h, 1D
    /// </summary>
    [HttpGet("{symbol}/candles")]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyCollection<CandleDto>>), 200)]
    public async Task<IActionResult> GetCandles(
        string symbol,
        [FromQuery] string interval = "1m",
        [FromQuery] int limit = 200,
        CancellationToken ct = default)
        => Ok(ApiResponse<IReadOnlyCollection<CandleDto>>.Ok(
            await _sender.Send(new GetCandlesQuery(symbol, interval, limit), ct)));
}