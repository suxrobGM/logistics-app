using FluentValidation;

namespace Logistics.Application.Modules.Tenancy.Tenants.Commands;

internal sealed class UpdateTenantPresetsValidator : AbstractValidator<UpdateTenantPresetsCommand>
{
    public UpdateTenantPresetsValidator()
    {
        RuleFor(i => i.Presets).MustBeValidPresets();
    }
}
