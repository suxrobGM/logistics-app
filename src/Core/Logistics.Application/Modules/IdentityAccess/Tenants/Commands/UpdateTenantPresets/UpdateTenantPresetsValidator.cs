using FluentValidation;
using Logistics.Domain.Entities;

namespace Logistics.Application.Modules.IdentityAccess.Tenants.Commands;

internal sealed class UpdateTenantPresetsValidator : AbstractValidator<UpdateTenantPresetsCommand>
{
    public UpdateTenantPresetsValidator()
    {
        RuleFor(i => i.Presets)
            .Must(p => TenantPresetCatalog.IsValid(p))
            .WithMessage(TenantPresetValidation.Message);
    }
}
