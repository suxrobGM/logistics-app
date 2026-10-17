using FluentValidation;

namespace Logistics.Application.Modules.Financial.Invoices.Queries;

internal sealed class PreviewPayrollInvoicesValidator : AbstractValidator<PreviewPayrollInvoicesQuery>
{
    public PreviewPayrollInvoicesValidator()
    {
        RuleFor(i => i.PeriodStart).LessThan(i => i.PeriodEnd);
        RuleFor(i => i.EmployeeIds).NotEmpty();
    }
}
