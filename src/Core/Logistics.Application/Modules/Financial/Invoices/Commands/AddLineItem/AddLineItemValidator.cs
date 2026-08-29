using FluentValidation;

namespace Logistics.Application.Modules.Financial.Invoices.Commands;

internal sealed class AddLineItemValidator : AbstractValidator<AddLineItemCommand>
{
    public AddLineItemValidator()
    {
        RuleFor(i => i.Description)
            .NotEmpty()
            .MaximumLength(500);

        // Guard the money fields that flow into the invoice total and tax math. A negative amount is
        // valid (credits/discounts), but bound the magnitude and require a positive quantity so an
        // unbounded/zero value can't corrupt the total or produce nonsensical tax.
        RuleFor(i => i.Amount)
            .GreaterThanOrEqualTo(-1_000_000m)
            .LessThanOrEqualTo(1_000_000m);

        RuleFor(i => i.Quantity)
            .GreaterThan(0)
            .LessThanOrEqualTo(100_000);
    }
}
