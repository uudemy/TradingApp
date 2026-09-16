using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TradingApp.Application.Abstractions;
using TradingApp.Application.Features.Auth.Dtos;
using TradingApp.Domain.Common;
using TradingApp.Domain.Entities;

namespace TradingApp.Application.Features.Auth.Refresh;

public sealed class RefreshTokenCommandHandler : IRequestHandler<RefreshTokenCommand, AuthResponse>
{
    private readonly IAppDbContext _db;
    private readonly IJwtTokenService _jwt;
    private readonly IDateTimeProvider _clock;
    private readonly ICurrentUserService _currentUser;
    private readonly ILogger<RefreshTokenCommandHandler> _logger;

    public RefreshTokenCommandHandler(
        IAppDbContext db, IJwtTokenService jwt, IDateTimeProvider clock,
        ICurrentUserService currentUser, ILogger<RefreshTokenCommandHandler> logger)
    {
        _db = db; _jwt = jwt; _clock = clock; _currentUser = currentUser; _logger = logger;
    }

    public async Task<AuthResponse> Handle(RefreshTokenCommand request, CancellationToken ct)
    {
        var hash = _jwt.HashRefreshToken(request.RefreshToken);

        var existing = await _db.RefreshTokens
            .FirstOrDefaultAsync(t => t.TokenHash == hash, ct)
            ?? throw new DomainException("invalid_refresh_token", "Refresh token is invalid.");

        if (!existing.IsActive)
            throw new DomainException("refresh_token_inactive", "Refresh token is expired or revoked.");

        var user = await _db.Users
            .Include(u => u.Role)
            .FirstOrDefaultAsync(u => u.Id == existing.UserId, ct)
            ?? throw new DomainException("user_missing", "User not found.");

        if (!user.IsActive)
            throw new DomainException("user_inactive", "User account is inactive.");

        // Rotation
        var newPlain = _jwt.CreateRefreshTokenPlain();
        var newHash = _jwt.HashRefreshToken(newPlain);
        var newExpires = _clock.UtcNow.AddDays(7);

        existing.Revoke(newHash, _currentUser.IpAddress);
        _db.RefreshTokens.Add(new RefreshToken(user.Id, newHash, newExpires, _currentUser.IpAddress));

        var roles = user.Role is null ? Array.Empty<string>() : new[] { user.Role.Name };
        var access = _jwt.CreateAccessToken(user.Id, user.Email, roles);

        await _db.SaveChangesAsync(ct);
        _logger.LogInformation("Refresh token rotated for user {UserId}", user.Id);

        var dto = new UserDto(user.Id, user.Email, user.FirstName, user.LastName,
            user.IsEmailVerified, roles);

        return new AuthResponse(access.Token, newPlain, access.ExpiresAt, newExpires, dto);
    }
}