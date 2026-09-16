using MediatR;
using Microsoft.EntityFrameworkCore;
using TradingApp.Application.Abstractions;
using TradingApp.Application.Features.Wallets.Dtos;
using TradingApp.Domain.Common;

namespace TradingApp.Application.Features.Wallets.GetWallets;

public sealed class GetWalletsQueryHandler
    : IRequestHandler<GetWalletsQuery, IReadOnlyCollection<WalletDto>>
{
    private readonly IAppDbContext _db;
    private readonly ICurrentUserService _currentUser;

    public GetWalletsQueryHandler(IAppDbContext db, ICurrentUserService currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task<IReadOnlyCollection<WalletDto>> Handle(
        GetWalletsQuery request, CancellationToken ct)
    {
        var userId = _currentUser.UserId
            ?? throw new DomainException("unauthorized", "Authentication required.");

        var wallets = await _db.Wallets.AsNoTracking()
            .Where(w => w.UserId == userId)
            .OrderBy(w => w.Currency)
            .ToListAsync(ct);

        return wallets.Select(w => new WalletDto(
            w.Id,
            w.Currency,
            w.AvailableBalance,
            w.LockedBalance,
            w.TotalBalance)).ToArray();
    }
}