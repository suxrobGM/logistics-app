using FluentValidation;

using Logistics.Application.Modules.Common.Constants;
using Logistics.Application.Validators;

namespace Logistics.Application.Modules.IdentityAccess.Tenants.Commands;

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

        RuleFor(i => i.VatNumber!)
            .Matches(RegexPatterns.VatNumber)
            .When(i => !string.IsNullOrWhiteSpace(i.VatNumber))
            .WithMessage("VAT number must be a 2-letter country prefix followed by 5–12 alphanumerics (e.g. DE123456789).");

        RuleFor(i => i.EoriNumber!)
            .Matches(RegexPatterns.EoriNumber)
            .When(i => !string.IsNullOrWhiteSpace(i.EoriNumber))
            .WithMessage("EORI number must be a 2-letter country prefix followed by 1–15 alphanumerics.");

        RuleFor(i => i.McNumber!)
            .Matches(RegexPatterns.McNumber)
            .When(i => !string.IsNullOrWhiteSpace(i.McNumber))
            .WithMessage("MC number must be 4–8 digits, optionally prefixed with 'MC' or 'MC-'.");

        RuleFor(i => i.TaxResidencyCountry!)
            .Length(2)
            .When(i => !string.IsNullOrWhiteSpace(i.TaxResidencyCountry))
            .WithMessage("Tax residency country must be a 2-letter ISO-3166-1 code.");

        RuleFor(i => i.CompanyAddress!)
            .SetValidator(new AddressValidator())
            .When(i => i.CompanyAddress is not null);
    }
}
