using FluentValidation;
using Logistics.Shared.Consts;

namespace Logistics.Application.Tenant.Commands;

internal sealed class CreateEmployeeValidator : AbstractValidator<CreateEmployeeCommand>
{
    public CreateEmployeeValidator()
    {
        RuleFor(i => i.UserId).NotEmpty();
        RuleFor(i => i.Salary).GreaterThanOrEqualTo(0M);
        When(i => i.SalaryType != SalaryType.None,
            () => RuleFor(i => i.Salary).GreaterThan(0M));
    }
}
