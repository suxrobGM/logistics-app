using FluentValidation;

namespace Logistics.Application.Modules.IdentityAccess.Employees.Commands;

internal sealed class DeleteEmployeeValidator : AbstractValidator<DeleteEmployeeCommand>
{
    public DeleteEmployeeValidator()
    {
        RuleFor(i => i.UserId).NotEmpty();
    }
}
