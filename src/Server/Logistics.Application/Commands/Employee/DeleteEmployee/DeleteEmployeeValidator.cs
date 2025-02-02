using FluentValidation;

namespace Logistics.Application.Commands;

internal sealed class DeleteEmployeeValidator : AbstractValidator<DeleteEmployeeCommand>
{
    public DeleteEmployeeValidator()
    {
        RuleFor(i => i.UserId).NotEmpty();
    }
}
