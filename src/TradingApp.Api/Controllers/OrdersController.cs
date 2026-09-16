using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TradingApp.Api.Common;
using TradingApp.Application.Features.Orders.CancelOrder;
using TradingApp.Application.Features.Orders.Dtos;
using TradingApp.Application.Features.Orders.PlaceOrder;
using TradingApp.Application.Features.Orders.GetOrders;
namespace TradingApp.Api.Controllers;

[ApiController]
[Route("api/orders")]
[Authorize]
public sealed class OrdersController : ControllerBase
{
    private readonly ISender _sender;
    public OrdersController(ISender sender) => _sender = sender;

    /// <summary>Yeni emir verir ve otomatik eşleştirir.</summary>
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<OrderDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Place([FromBody] PlaceOrderCommand cmd, CancellationToken ct)
        => Ok(ApiResponse<OrderDto>.Ok(await _sender.Send(cmd, ct)));

    /// <summary>Açık emri iptal eder ve kilitli bakiyeyi çözer.</summary>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Cancel(Guid id, CancellationToken ct)
    {
        await _sender.Send(new CancelOrderCommand(id), ct);
        return NoContent();
    }

    /// <summary>Kullanıcının emirlerini listeler.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyCollection<OrderDto>>), 200)]
    public async Task<IActionResult> GetAll(
        [FromQuery] string? status,
        [FromQuery] string? symbol,
        CancellationToken ct)
        => Ok(ApiResponse<IReadOnlyCollection<OrderDto>>.Ok(
            await _sender.Send(new GetOrdersQuery(status, symbol), ct)));
}