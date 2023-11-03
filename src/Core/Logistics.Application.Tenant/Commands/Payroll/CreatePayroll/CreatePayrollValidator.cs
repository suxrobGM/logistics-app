using FluentValidation;

namespace Logistics.Application.Tenant.Commands;

internal sealed class CreatePayrollValidator : AbstractValidator<CreatePayrollCommand>
{
    public CreatePayrollValidator()
    {
        RuleFor(i => i.StartDate).LessThan(i => i.EndDate);
        RuleFor(i => i.EmployeeId).NotEmpty();
    }
}
