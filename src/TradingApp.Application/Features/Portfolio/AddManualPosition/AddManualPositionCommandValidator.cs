using FluentValidation;

namespace TradingApp.Application.Features.Portfolio.AddManualPosition;

public sealed class AddManualPositionCommandValidator : AbstractValidator<AddManualPositionCommand>
{
    public AddManualPositionCommandValidator()
    {
        RuleFor(x => x.Symbol).NotEmpty().MaximumLength(20);
        RuleFor(x => x.Quantity).GreaterThan(0m);
        RuleFor(x => x.AverageCost).GreaterThan(0m);
        RuleFor(x => x.Notes).MaximumLength(500);
    }
}