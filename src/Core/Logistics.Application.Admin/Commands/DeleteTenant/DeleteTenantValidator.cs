using FluentValidation;

namespace Logistics.Application.Admin.Commands;

internal sealed class DeleteTenantValidator : AbstractValidator<DeleteTenantCommand>
{
    public DeleteTenantValidator()
    {
        RuleFor(i => i.Id)
            .NotEmpty();
    }
}