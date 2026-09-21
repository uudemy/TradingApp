using MediatR;
using Microsoft.EntityFrameworkCore;
using TradingApp.Application.Abstractions;
using TradingApp.Application.Features.PriceAlerts.Dtos;
using TradingApp.Domain.Common;
using TradingApp.Domain.Entities;

namespace TradingApp.Application.Features.PriceAlerts.CreatePriceAlert;

public sealed class CreatePriceAlertCommandHandler
    : IRequestHandler<CreatePriceAlertCommand, PriceAlertDto>
{
    private readonly IAppDbContext _db;
    private readonly ICurrentUserService _currentUser;

    public CreatePriceAlertCommandHandler(IAppDbContext db, ICurrentUserService currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task<PriceAlertDto> Handle(CreatePriceAlertCommand request, CancellationToken ct)
    {
        var userId = _currentUser.UserId
            ?? throw new DomainException("unauthorized", "Authentication required.");

        var symbol = request.Symbol.Trim().ToUpperInvariant();

        var asset = await _db.Assets.FirstOrDefaultAsync(a => a.Symbol == symbol, ct)
            ?? throw new DomainException("asset_not_found", $"Asset '{symbol}' not found.");

        // Aynı yönde aktif alarm zaten varsa engelle
        var duplicate = await _db.PriceAlerts.AnyAsync(p =>
            p.UserId == userId
            && p.AssetId == asset.Id
            && p.Condition == request.Condition
            && p.TargetPrice == request.TargetPrice
            && p.IsActive, ct);

        if (duplicate)
            throw new DomainException("alert_exists", "Identical active alert already exists.");

        var alert = new PriceAlert(userId, asset.Id, request.Condition, request.TargetPrice);
        _db.PriceAlerts.Add(alert);
        await _db.SaveChangesAsync(ct);

        return new PriceAlertDto(
            alert.Id, asset.Id, symbol,
            alert.Condition.ToString(), alert.TargetPrice,
            alert.IsActive, alert.TriggeredAt, alert.CreatedAt);
    }
}