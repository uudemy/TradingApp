using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TradingApp.Application.Abstractions;
using TradingApp.Application.Features.Auth.Dtos;
using TradingApp.Domain.Common;
using TradingApp.Domain.Entities;
using TradingApp.Domain.Enums;
using PortfolioEntity = TradingApp.Domain.Entities.Portfolio;
namespace TradingApp.Application.Features.Auth.Register;

public sealed class RegisterCommandHandler : IRequestHandler<RegisterCommand, AuthResponse>
{
    // Demo başlangıç bakiyeleri
    private static readonly (string Currency, decimal Amount)[] StartingBalances =
    {
        ("TRY",  100_000m),
        ("USD",   10_000m),
        ("USDT",  10_000m),
    };

    private readonly IAppDbContext _db;
    private readonly IPasswordHasher _hasher;
    private readonly IJwtTokenService _jwt;
    private readonly IDateTimeProvider _clock;
    private readonly ICurrentUserService _currentUser;
    private readonly ILogger<RegisterCommandHandler> _logger;

    public RegisterCommandHandler(
        IAppDbContext db,
        IPasswordHasher hasher,
        IJwtTokenService jwt,
        IDateTimeProvider clock,
        ICurrentUserService currentUser,
        ILogger<RegisterCommandHandler> logger)
    {
        _db = db;
        _hasher = hasher;
        _jwt = jwt;
        _clock = clock;
        _currentUser = currentUser;
        _logger = logger;
    }

    public async Task<AuthResponse> Handle(RegisterCommand request, CancellationToken ct)
    {
        var email = request.Email.Trim().ToLowerInvariant();

        if (await _db.Users.AnyAsync(u => u.Email == email, ct))
            throw new DomainException("email_taken", "This email is already registered.");

        var userRole = await _db.Roles.FirstOrDefaultAsync(r => r.Name == "User", ct)
            ?? throw new DomainException("role_missing", "Default 'User' role not found. Did seed run?");

        var hash = _hasher.Hash(request.Password);
        var user = new User(email, hash, request.FirstName, request.LastName);
        user.AssignRole(userRole.Id);

        _db.Users.Add(user);

        // Demo cüzdanlar
        foreach (var (currency, amount) in StartingBalances)
        {
            _db.Wallets.Add(new Wallet(user.Id, currency, amount));
        }

        // Boş portfolio
        // Boş portfolio
        _db.Portfolios.Add(new PortfolioEntity(user.Id));
        // Token'lar
        var access = _jwt.CreateAccessToken(user.Id, user.Email, new[] { userRole.Name });
        var refreshPlain = _jwt.CreateRefreshTokenPlain();
        var refreshHash = _jwt.HashRefreshToken(refreshPlain);
        var refreshExpires = _clock.UtcNow.AddDays(7);

        _db.RefreshTokens.Add(new RefreshToken(user.Id, refreshHash, refreshExpires, _currentUser.IpAddress));

        await _db.SaveChangesAsync(ct);

        _logger.LogInformation("User registered: {UserId} {Email}", user.Id, user.Email);

        var dto = new UserDto(user.Id, user.Email, user.FirstName, user.LastName, user.IsEmailVerified,
            new[] { userRole.Name });

        return new AuthResponse(access.Token, refreshPlain, access.ExpiresAt, refreshExpires, dto);
    }
}