using FluentValidation;

namespace Logistics.Application.Modules.IdentityAccess.Tenants.Commands;

internal sealed class DeleteTenantValidator : AbstractValidator<DeleteTenantCommand>
{
    public DeleteTenantValidator()
    {
        RuleFor(i => i.Id).NotEmpty();
    }
}
