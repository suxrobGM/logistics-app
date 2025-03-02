using FluentValidation;
using Logistics.Shared.Consts;

namespace Logistics.Application.Commands;

internal sealed class UpdateTenantValidator : AbstractValidator<UpdateTenantCommand>
{
    public UpdateTenantValidator()
    {
        RuleFor(i => i.Id)
            .NotEmpty();
        
        RuleFor(i => i.Name)
            .MinimumLength(4)
            .Matches(RegexPatterns.TenantName);
        
        RuleFor(i => i.BillingEmail)
            .EmailAddress();
    }
}
