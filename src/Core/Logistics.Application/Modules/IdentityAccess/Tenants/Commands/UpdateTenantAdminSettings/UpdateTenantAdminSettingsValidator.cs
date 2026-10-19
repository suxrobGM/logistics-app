using FluentValidation;
using Logistics.Application.Modules.Common.Constants;

namespace Logistics.Application.Modules.IdentityAccess.Tenants.Commands;

internal sealed class UpdateTenantAdminSettingsValidator : AbstractValidator<UpdateTenantAdminSettingsCommand>
{
    public UpdateTenantAdminSettingsValidator()
    {
        RuleFor(i => i.Name)
            .MinimumLength(4)
            .Matches(RegexPatterns.TenantName);
    }
}
