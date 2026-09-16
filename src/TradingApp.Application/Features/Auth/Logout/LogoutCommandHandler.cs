using MediatR;
using Microsoft.EntityFrameworkCore;
using TradingApp.Application.Abstractions;

namespace TradingApp.Application.Features.Auth.Logout;

public sealed class LogoutCommandHandler : IRequestHandler<LogoutCommand>
{
    private readonly IAppDbContext _db;
    private readonly IJwtTokenService _jwt;
    private readonly ICurrentUserService _currentUser;

    public LogoutCommandHandler(IAppDbContext db, IJwtTokenService jwt, ICurrentUserService currentUser)
    {
        _db = db; _jwt = jwt; _currentUser = currentUser;
    }

    public async Task Handle(LogoutCommand request, CancellationToken ct)
    {
        var hash = _jwt.HashRefreshToken(request.RefreshToken);
        var token = await _db.RefreshTokens.FirstOrDefaultAsync(t => t.TokenHash == hash, ct);

        // Idempotent: zaten yoksa veya revoke ise hata fırlatmıyoruz
        if (token is null || token.RevokedAt is not null) return;

        token.Revoke(revokedByIp: _currentUser.IpAddress);
        await _db.SaveChangesAsync(ct);
    }
}