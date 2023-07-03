using FluentValidation;

namespace Logistics.Application.Admin.Commands;

internal class DeleteTenantValidator : AbstractValidator<DeleteTenantCommand>
{
    public DeleteTenantValidator()
    {
        RuleFor(i => i.Id)
            .NotEmpty();
    }
}