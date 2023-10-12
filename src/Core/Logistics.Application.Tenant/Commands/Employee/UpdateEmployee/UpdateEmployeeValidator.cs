using FluentValidation;

namespace Logistics.Application.Tenant.Commands;

internal sealed class UpdateEmployeeValidator : AbstractValidator<UpdateEmployeeCommand>
{
    public UpdateEmployeeValidator()
    {
        RuleFor(i => i.UserId).NotEmpty();
    }
}
