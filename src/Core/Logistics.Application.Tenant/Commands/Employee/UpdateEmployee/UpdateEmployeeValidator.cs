using FluentValidation;
using Logistics.Shared.Consts;

namespace Logistics.Application.Tenant.Commands;

internal sealed class UpdateEmployeeValidator : AbstractValidator<UpdateEmployeeCommand>
{
    public UpdateEmployeeValidator()
    {
        RuleFor(i => i.UserId).NotEmpty();
        
        When(i => i.SalaryType.HasValue, () =>
            {
                When(i => i.SalaryType == SalaryType.ShareOfGross, () => RuleFor(i => i.Salary)
                    .NotEmpty()
                    .InclusiveBetween(0, 1));
                
                When(i => i.SalaryType != SalaryType.ShareOfGross , () => RuleFor(i => i.Salary)
                    .NotEmpty()
                    .GreaterThanOrEqualTo(0M));
            }
        );
    }
}
