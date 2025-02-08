using FluentValidation;
using Logistics.Shared.Consts;

namespace Logistics.Application.Commands;

internal sealed class UpdateSubscriptionPlanValidator : AbstractValidator<UpdateTenantCommand>
{
    public UpdateSubscriptionPlanValidator()
    {
        RuleFor(i => i.Id)
            .NotEmpty();
        
        RuleFor(i => i.Name)
            .NotEmpty()
            .MinimumLength(4)
            .Matches(RegexPatterns.TenantName);
    }
}
