using FluentValidation;
using Logistics.Application.Modules.Common.Constants;
using Logistics.Application.Modules.IdentityAccess.Tenants.Constants;

namespace Logistics.Application.Modules.IdentityAccess.Tenants.Commands;

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
