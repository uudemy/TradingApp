namespace TradingApp.Application.Abstractions;

public sealed record AccessTokenResult(string Token, DateTimeOffset ExpiresAt);

public interface IJwtTokenService
{
    AccessTokenResult CreateAccessToken(
        Guid userId,
        string email,
        IEnumerable<string> roles);

    /// <summary>Refresh token için kriptografik rastgele plain token üretir.</summary>
    string CreateRefreshTokenPlain();

    /// <summary>Refresh token'ı deterministik hash'ler (SHA256/Base64).</summary>
    string HashRefreshToken(string plainToken);
}