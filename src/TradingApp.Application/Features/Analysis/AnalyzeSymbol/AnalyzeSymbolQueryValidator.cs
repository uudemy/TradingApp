using FluentValidation;

namespace TradingApp.Application.Features.Analysis.AnalyzeSymbol;

public sealed class AnalyzeSymbolQueryValidator : AbstractValidator<AnalyzeSymbolQuery>
{
    public AnalyzeSymbolQueryValidator()
    {
        RuleFor(x => x.Symbol).NotEmpty().MaximumLength(20);
        RuleFor(x => x.Interval).NotEmpty();
        RuleFor(x => x.Limit).InclusiveBetween(50, 500);
    }
}