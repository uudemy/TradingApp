using FluentValidation;

namespace TradingApp.Application.Features.Portfolio.UpdatePosition;

public sealed class UpdatePositionCommandValidator : AbstractValidator<UpdatePositionCommand>
{
    public UpdatePositionCommandValidator()
    {
        RuleFor(x => x.PositionId).NotEmpty();
        RuleFor(x => x.Quantity).GreaterThanOrEqualTo(0m);
        RuleFor(x => x.AverageCost).GreaterThanOrEqualTo(0m);
        RuleFor(x => x.Notes).MaximumLength(500);
    }
}