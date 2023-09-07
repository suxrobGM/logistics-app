using FluentValidation;

namespace Logistics.Application.Tenant.Commands;

internal sealed class DeleteEmployeeValidator : AbstractValidator<DeleteEmployeeCommand>
{
    public DeleteEmployeeValidator()
    {
        RuleFor(i => i.UserId).NotEmpty();
    }
}
