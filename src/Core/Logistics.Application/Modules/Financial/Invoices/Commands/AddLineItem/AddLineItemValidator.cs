using FluentValidation;

namespace Logistics.Application.Modules.Financial.Invoices.Commands;

internal sealed class AddLineItemValidator : AbstractValidator<AddLineItemCommand>
{
    public AddLineItemValidator()
    {
        RuleFor(i => i.Description)
            .NotEmpty()
            .MaximumLength(500);

        // A negative amount is a credit or discount, so only the quantity has a floor.
        RuleFor(i => i.Quantity)
            .GreaterThan(0);
    }
}
