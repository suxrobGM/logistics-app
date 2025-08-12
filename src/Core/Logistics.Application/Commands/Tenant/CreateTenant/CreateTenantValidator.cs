using FluentValidation;

using Logistics.Application.Constants;

namespace Logistics.Application.Commands;

internal sealed class CreateTenantValidator : AbstractValidator<CreateTenantCommand>
{
    public CreateTenantValidator()
    {
        RuleFor(i => i.Name)
            .NotEmpty()
            .MinimumLength(4)
            .Matches(RegexPatterns.TenantName);

        RuleFor(i => i.BillingEmail)
            .NotEmpty()
            .EmailAddress();
    }
}
