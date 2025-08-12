using FluentValidation;

namespace Logistics.Application.Commands;

internal sealed class UpdateInvoiceValidator : AbstractValidator<UpdatePaymentCommand>
{
    public UpdateInvoiceValidator()
    {
        RuleFor(i => i.Id).NotEmpty();
    }
}
