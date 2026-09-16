using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using TradingApp.Application.Abstractions;

namespace TradingApp.Infrastructure.Identity;

public sealed class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _accessor;

    public CurrentUserService(IHttpContextAccessor accessor) => _accessor = accessor;

    public Guid? UserId
    {
        get
        {
            var user = _accessor.HttpContext?.User;
            if (user is null) return null;

            // Önce NameIdentifier, sonra "sub" (JWT standardı)
            var raw = user.FindFirst(ClaimTypes.NameIdentifier)?.Value
                      ?? user.FindFirst("sub")?.Value;

            return Guid.TryParse(raw, out var id) ? id : null;
        }
    }

    public string? Email
        => _accessor.HttpContext?.User?.FindFirst(ClaimTypes.Email)?.Value
           ?? _accessor.HttpContext?.User?.FindFirst("email")?.Value;

    public bool IsAuthenticated
        => _accessor.HttpContext?.User?.Identity?.IsAuthenticated == true;

    public string? IpAddress
        => _accessor.HttpContext?.Connection.RemoteIpAddress?.ToString();
}