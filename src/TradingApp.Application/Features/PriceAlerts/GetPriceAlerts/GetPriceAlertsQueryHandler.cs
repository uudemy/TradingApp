using MediatR;
using Microsoft.EntityFrameworkCore;
using TradingApp.Application.Abstractions;
using TradingApp.Application.Features.PriceAlerts.Dtos;
using TradingApp.Domain.Common;

namespace TradingApp.Application.Features.PriceAlerts.GetPriceAlerts;

public sealed class GetPriceAlertsQueryHandler
    : IRequestHandler<GetPriceAlertsQuery, IReadOnlyCollection<PriceAlertDto>>
{
    private readonly IAppDbContext _db;
    private readonly ICurrentUserService _currentUser;

    public GetPriceAlertsQueryHandler(IAppDbContext db, ICurrentUserService currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task<IReadOnlyCollection<PriceAlertDto>> Handle(
        GetPriceAlertsQuery request, CancellationToken ct)
    {
        var userId = _currentUser.UserId
            ?? throw new DomainException("unauthorized", "Authentication required.");

        var query = _db.PriceAlerts.AsNoTracking().Where(p => p.UserId == userId);

        if (request.ActiveOnly == true)
            query = query.Where(p => p.IsActive);

        var alerts = await query.OrderByDescending(p => p.CreatedAt).ToListAsync(ct);

        var assetIds = alerts.Select(a => a.AssetId).Distinct().ToList();
        var symbols = await _db.Assets
            .Where(a => assetIds.Contains(a.Id))
            .ToDictionaryAsync(a => a.Id, a => a.Symbol, ct);

        return alerts.Select(a => new PriceAlertDto(
            a.Id,
            a.AssetId,
            symbols.GetValueOrDefault(a.AssetId, "?"),
            a.Condition.ToString(),
            a.TargetPrice,
            a.IsActive,
            a.TriggeredAt,
            a.CreatedAt)).ToArray();
    }
}