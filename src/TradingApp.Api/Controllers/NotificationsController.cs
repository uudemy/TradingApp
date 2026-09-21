using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TradingApp.Api.Common;
using TradingApp.Application.Features.Notifications.Dtos;
using TradingApp.Application.Features.Notifications.GetNotifications;
using TradingApp.Application.Features.Notifications.MarkNotificationRead;

namespace TradingApp.Api.Controllers;

[ApiController]
[Route("api/notifications")]
[Authorize]
public sealed class NotificationsController : ControllerBase
{
    private readonly ISender _sender;
    public NotificationsController(ISender sender) => _sender = sender;

    /// <summary>Kullanıcının bildirimlerini listeler.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyCollection<NotificationDto>>), 200)]
    public async Task<IActionResult> Get(
        [FromQuery] bool unreadOnly = false,
        CancellationToken ct = default)
        => Ok(ApiResponse<IReadOnlyCollection<NotificationDto>>.Ok(
            await _sender.Send(new GetNotificationsQuery(unreadOnly), ct)));

    /// <summary>Bildirimi okundu olarak işaretler.</summary>
    [HttpPut("{id:guid}/read")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> MarkRead(Guid id, CancellationToken ct)
    {
        await _sender.Send(new MarkNotificationReadCommand(id), ct);
        return NoContent();
    }
}