using FluentValidation;
using Logistics.Application.Common.Constants;
using Logistics.Application.Modules.Tenancy.Tenants.Constants;

namespace Logistics.Application.Modules.Tenancy.Tenants.Commands;

internal sealed class UpdateTenantAdminSettingsValidator : AbstractValidator<UpdateTenantAdminSettingsCommand>
{
    public UpdateTenantAdminSettingsValidator()
    {
        RuleFor(i => i.Name)
            .MinimumLength(4)
            .MaximumLength(TenantConstants.NameLength)
            .Matches(RegexPatterns.TenantName);
    }
}
