using FluentValidation;
using Logistics.Application.Validators;

namespace Logistics.Application.Modules.IdentityAccess.Tenants.Commands;

internal sealed class UpdateTenantPresetsValidator : AbstractValidator<UpdateTenantPresetsCommand>
{
    public UpdateTenantPresetsValidator()
    {
        RuleFor(i => i.Presets).MustBeValidPresets();
    }
}
