using FluentValidation;
using Logistics.Shared.Models;

namespace Logistics.Application.Commands;

internal sealed class CreateSubscriptionValidator : AbstractValidator<CreateSubscriptionCommand>
{
    public CreateSubscriptionValidator()
    {
        RuleFor(i => i.PlanId).NotEmpty();
        RuleFor(i => i.TenantId).NotEmpty();
    }
}
