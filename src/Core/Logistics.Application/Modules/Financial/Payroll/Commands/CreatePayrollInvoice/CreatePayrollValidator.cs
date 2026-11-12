using FluentValidation;

namespace Logistics.Application.Modules.Financial.Payroll.Commands;

internal sealed class CreatePayrollValidator : AbstractValidator<CreatePayrollInvoiceCommand>
{
    public CreatePayrollValidator()
    {
        RuleFor(i => i.PeriodStart).LessThan(i => i.PeriodEnd);
        RuleFor(i => i.EmployeeId).NotEmpty();
    }
}
