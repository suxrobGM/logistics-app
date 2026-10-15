using FluentValidation;
using Logistics.Shared.Models;

namespace Logistics.Application.Modules.Operations.Loads.Commands;

internal sealed class LoadAccessorialValidator : AbstractValidator<LoadAccessorialDto>
{
    public LoadAccessorialValidator()
    {
        RuleFor(a => a.Name).NotEmpty().MaximumLength(200);
        RuleFor(a => a.LineItemType).IsInEnum();
        RuleFor(a => a.Unit).IsInEnum();
        RuleFor(a => a.Rate).GreaterThanOrEqualTo(0);
        RuleFor(a => a.Quantity).GreaterThan(0);
    }
}
