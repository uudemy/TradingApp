using MediatR;
using Microsoft.EntityFrameworkCore;
using TradingApp.Application.Abstractions;
using TradingApp.Application.Features.Notifications.Dtos;
using TradingApp.Domain.Common;

namespace TradingApp.Application.Features.Notifications.GetNotifications;

public sealed class GetNotificationsQueryHandler
    : IRequestHandler<GetNotificationsQuery, IReadOnlyCollection<NotificationDto>>
{
    private readonly IAppDbContext _db;
    private readonly ICurrentUserService _currentUser;

    public GetNotificationsQueryHandler(IAppDbContext db, ICurrentUserService currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task<IReadOnlyCollection<NotificationDto>> Handle(
        GetNotificationsQuery request, CancellationToken ct)
    {
        var userId = _currentUser.UserId
            ?? throw new DomainException("unauthorized", "Authentication required.");

        var query = _db.Notifications.AsNoTracking().Where(n => n.UserId == userId);
        if (request.UnreadOnly)
            query = query.Where(n => !n.IsRead);

        var rows = await query
            .OrderByDescending(n => n.CreatedAt)
            .Take(200)
            .ToListAsync(ct);

        return rows.Select(n => new NotificationDto(
            n.Id, n.Title, n.Message, n.Type.ToString(),
            n.IsRead, n.CreatedAt)).ToArray();
    }
}