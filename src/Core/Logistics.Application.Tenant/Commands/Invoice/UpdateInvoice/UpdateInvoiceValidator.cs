using FluentValidation;

namespace Logistics.Application.Tenant.Commands;

internal sealed class UpdateInvoiceValidator : AbstractValidator<UpdatePaymentCommand>
{
    public UpdateInvoiceValidator()
    {
        RuleFor(i => i.Id).NotEmpty();
    }
}
