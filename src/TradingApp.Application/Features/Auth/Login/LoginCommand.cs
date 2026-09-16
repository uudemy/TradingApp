using MediatR;
using TradingApp.Application.Features.Auth.Dtos;

namespace TradingApp.Application.Features.Auth.Login;

public sealed record LoginCommand(string Email, string Password) : IRequest<AuthResponse>;