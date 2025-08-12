using FluentValidation;

namespace Logistics.Application.Commands;

internal sealed class UpdatePayrollInvoiceValidator : AbstractValidator<UpdatePayrollInvoiceCommand>
{
    public UpdatePayrollInvoiceValidator()
    {
        RuleFor(i => i.PeriodStart).LessThan(i => i.PeriodEnd);
        RuleFor(i => i.Id).NotEmpty();
    }
}
