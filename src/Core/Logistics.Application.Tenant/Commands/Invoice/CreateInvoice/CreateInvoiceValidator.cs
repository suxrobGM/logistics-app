using FluentValidation;

namespace Logistics.Application.Tenant.Commands;

internal sealed class CreateInvoiceValidator : AbstractValidator<CreateInvoiceCommand>
{
    public CreateInvoiceValidator()
    {
        RuleFor(i => i.CustomerId).NotEmpty();
        RuleFor(i => i.LoadId).NotEmpty();
        RuleFor(i => i.PaymentAmount).GreaterThan(0M);
    }
}
