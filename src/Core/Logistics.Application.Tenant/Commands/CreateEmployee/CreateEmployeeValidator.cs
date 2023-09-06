using FluentValidation;

namespace Logistics.Application.Tenant.Commands;

internal sealed class CreateEmployeeValidator : AbstractValidator<CreateEmployeeCommand>
{
    public CreateEmployeeValidator()
    {
        RuleFor(i => i.Id).NotEmpty();
    }
}
