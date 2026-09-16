using MediatR;
using TradingApp.Application.Features.Auth.Dtos;

namespace TradingApp.Application.Features.Auth.Register;

public sealed record RegisterCommand(
    string Email,
    string Password,
    string FirstName,
    string LastName) : IRequest<AuthResponse>;