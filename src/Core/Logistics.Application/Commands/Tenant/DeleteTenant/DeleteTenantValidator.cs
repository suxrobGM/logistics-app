using FluentValidation;

namespace Logistics.Application.Commands;

internal sealed class DeleteTenantValidator : AbstractValidator<DeleteTenantCommand>
{
    public DeleteTenantValidator()
    {
        RuleFor(i => i.Id).NotEmpty();
    }
}
