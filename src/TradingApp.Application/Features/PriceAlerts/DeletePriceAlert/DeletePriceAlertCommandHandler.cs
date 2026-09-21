using MediatR;
using Microsoft.EntityFrameworkCore;
using TradingApp.Application.Abstractions;
using TradingApp.Domain.Common;

namespace TradingApp.Application.Features.PriceAlerts.DeletePriceAlert;

public sealed class DeletePriceAlertCommandHandler : IRequestHandler<DeletePriceAlertCommand>
{
    private readonly IAppDbContext _db;
    private readonly ICurrentUserService _currentUser;

    public DeletePriceAlertCommandHandler(IAppDbContext db, ICurrentUserService currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task Handle(DeletePriceAlertCommand request, CancellationToken ct)
    {
        var userId = _currentUser.UserId
            ?? throw new DomainException("unauthorized", "Authentication required.");

        var alert = await _db.PriceAlerts
            .FirstOrDefaultAsync(p => p.Id == request.AlertId, ct)
            ?? throw new DomainException("alert_not_found", "Alert not found.");

        if (alert.UserId != userId)
            throw new DomainException("forbidden", "You can only delete your own alerts.");

        _db.PriceAlerts.Remove(alert);
        await _db.SaveChangesAsync(ct);
    }
}