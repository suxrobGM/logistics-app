using FluentValidation;
using Logistics.Application.Modules.Common.Constants;
using Logistics.Application.Validators;

namespace Logistics.Application.Modules.IdentityAccess.Tenants.Commands;

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

        RuleFor(i => i.OwnerEmail)
            .NotEmpty()
            .EmailAddress();

        RuleFor(i => i.OwnerFirstName)
            .NotEmpty();

        RuleFor(i => i.OwnerLastName)
            .NotEmpty();

        RuleFor(i => i.CompanyAddress)
            .NotNull()
            .SetValidator(new AddressValidator());
    }
}
