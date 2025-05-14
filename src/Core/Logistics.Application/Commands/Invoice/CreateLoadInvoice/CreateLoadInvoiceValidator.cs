using FluentValidation;

namespace Logistics.Application.Commands;

internal sealed class CreateLoadInvoiceValidator : AbstractValidator<CreateLoadInvoiceCommand>
{
    public CreateLoadInvoiceValidator()
    {
        RuleFor(i => i.CustomerId).NotEmpty();
        RuleFor(i => i.LoadId).NotEmpty();
        RuleFor(i => i.PaymentMethodId).NotEmpty();
        RuleFor(i => i.PaymentAmount).GreaterThan(0M);
    }
}
