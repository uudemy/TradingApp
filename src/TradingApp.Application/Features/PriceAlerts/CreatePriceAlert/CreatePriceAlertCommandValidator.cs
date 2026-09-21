using FluentValidation;

namespace TradingApp.Application.Features.PriceAlerts.CreatePriceAlert;

public sealed class CreatePriceAlertCommandValidator : AbstractValidator<CreatePriceAlertCommand>
{
    public CreatePriceAlertCommandValidator()
    {
        RuleFor(x => x.Symbol).NotEmpty().MaximumLength(20);
        RuleFor(x => x.TargetPrice).GreaterThan(0m);
        RuleFor(x => x.Condition).IsInEnum();
    }
}