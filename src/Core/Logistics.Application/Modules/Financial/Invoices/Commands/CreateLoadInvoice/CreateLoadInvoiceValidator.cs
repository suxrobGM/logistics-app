using FluentValidation;

namespace Logistics.Application.Modules.Financial.Invoices.Commands;

internal sealed class CreateLoadInvoiceValidator : AbstractValidator<CreateLoadInvoiceCommand>
{
    public CreateLoadInvoiceValidator()
    {
        RuleFor(i => i.CustomerId).NotEmpty();
        RuleFor(i => i.LoadId).NotEmpty();
        RuleFor(i => i.PaymentAmount).GreaterThan(0M);
    }
}
