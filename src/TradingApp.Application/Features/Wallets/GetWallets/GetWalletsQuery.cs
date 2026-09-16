using MediatR;
using TradingApp.Application.Features.Wallets.Dtos;

namespace TradingApp.Application.Features.Wallets.GetWallets;

public sealed record GetWalletsQuery : IRequest<IReadOnlyCollection<WalletDto>>;