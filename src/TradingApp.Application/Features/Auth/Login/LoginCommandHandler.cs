using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TradingApp.Application.Abstractions;
using TradingApp.Application.Features.Auth.Dtos;
using TradingApp.Domain.Common;
using TradingApp.Domain.Entities;

namespace TradingApp.Application.Features.Auth.Login;

public sealed class LoginCommandHandler : IRequestHandler<LoginCommand, AuthResponse>
{
    private readonly IAppDbContext _db;
    private readonly IPasswordHasher _hasher;
    private readonly IJwtTokenService _jwt;
    private readonly IDateTimeProvider _clock;
    private readonly ICurrentUserService _currentUser;
    private readonly ILogger<LoginCommandHandler> _logger;

    public LoginCommandHandler(
        IAppDbContext db, IPasswordHasher hasher, IJwtTokenService jwt,
        IDateTimeProvider clock, ICurrentUserService currentUser,
        ILogger<LoginCommandHandler> logger)
    {
        _db = db; _hasher = hasher; _jwt = jwt; _clock = clock;
        _currentUser = currentUser; _logger = logger;
    }

    public async Task<AuthResponse> Handle(LoginCommand request, CancellationToken ct)
    {
        var email = request.Email.Trim().ToLowerInvariant();

        var user = await _db.Users
            .Include(u => u.Role)
            .FirstOrDefaultAsync(u => u.Email == email, ct);

        // Güvenlik: kullanıcı var mı yok mu sızdırma — aynı mesaj
        if (user is null || !_hasher.Verify(request.Password, user.PasswordHash))
            throw new DomainException("invalid_credentials", "Email or password is incorrect.");

        if (!user.IsActive)
            throw new DomainException("user_inactive", "User account is inactive.");

        var roles = user.Role is null ? Array.Empty<string>() : new[] { user.Role.Name };
        var access = _jwt.CreateAccessToken(user.Id, user.Email, roles);
        var refreshPlain = _jwt.CreateRefreshTokenPlain();
        var refreshHash = _jwt.HashRefreshToken(refreshPlain);
        var refreshExpires = _clock.UtcNow.AddDays(7);

        _db.RefreshTokens.Add(new RefreshToken(user.Id, refreshHash, refreshExpires, _currentUser.IpAddress));
        await _db.SaveChangesAsync(ct);

        _logger.LogInformation("User logged in: {UserId} {Email}", user.Id, user.Email);

        var dto = new UserDto(user.Id, user.Email, user.FirstName, user.LastName,
            user.IsEmailVerified, roles);

        return new AuthResponse(access.Token, refreshPlain, access.ExpiresAt, refreshExpires, dto);
    }
}