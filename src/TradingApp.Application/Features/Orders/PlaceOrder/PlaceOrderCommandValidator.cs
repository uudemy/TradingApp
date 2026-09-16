using FluentValidation;
using TradingApp.Domain.Enums;

namespace TradingApp.Application.Features.Orders.PlaceOrder;

public sealed class PlaceOrderCommandValidator : AbstractValidator<PlaceOrderCommand>
{
    public PlaceOrderCommandValidator()
    {
        RuleFor(x => x.Symbol)
            .NotEmpty().WithMessage("Symbol required.");

        RuleFor(x => x.Quantity)
            .GreaterThan(0m).WithMessage("Quantity must be positive.");

        RuleFor(x => x.Price)
            .GreaterThan(0m).When(x => x.OrderType == OrderType.Limit)
            .WithMessage("Limit price must be positive.");

        RuleFor(x => x.Price)
            .Null().When(x => x.OrderType == OrderType.Market)
            .WithMessage("Market orders must not include a price.");
    }
}