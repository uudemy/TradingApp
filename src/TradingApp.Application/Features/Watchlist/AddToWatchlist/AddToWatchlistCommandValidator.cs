using FluentValidation;

namespace TradingApp.Application.Features.Watchlist.AddToWatchlist;

public sealed class AddToWatchlistCommandValidator : AbstractValidator<AddToWatchlistCommand>
{
    public AddToWatchlistCommandValidator()
    {
        RuleFor(x => x.Symbol).NotEmpty().MaximumLength(20);
    }
}