using MediatR;
using TradingApp.Application.Features.Auth.Dtos;

namespace TradingApp.Application.Features.Auth.Refresh;

public sealed record RefreshTokenCommand(string RefreshToken) : IRequest<AuthResponse>;