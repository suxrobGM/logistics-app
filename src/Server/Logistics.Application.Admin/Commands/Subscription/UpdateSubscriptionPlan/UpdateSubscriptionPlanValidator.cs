using FluentValidation;
using Logistics.Shared;

namespace Logistics.Application.Admin.Commands;

internal sealed class UpdateSubscriptionPlanValidator : AbstractValidator<UpdateTenantCommand>
{
    public UpdateSubscriptionPlanValidator()
    {
        RuleFor(i => i.Id)
            .NotEmpty();
        
        RuleFor(i => i.Name)
            .NotEmpty()
            .MinimumLength(4)
            .Matches(RegexPatterns.TENANT_NAME);
    }
}
