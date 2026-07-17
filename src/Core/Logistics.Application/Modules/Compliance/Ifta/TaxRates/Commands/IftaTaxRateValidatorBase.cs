using FluentValidation;

namespace Logistics.Application.Modules.Compliance.Ifta.TaxRates.Commands;

internal abstract class IftaTaxRateValidatorBase<T> : AbstractValidator<T> where T : IIftaTaxRateFields
{
    protected IftaTaxRateValidatorBase()
    {
        RuleFor(x => x.CountryCode)
            .NotEmpty()
            .Length(2);

        RuleFor(x => x.Region)
            .MaximumLength(10);

        RuleFor(x => x.Year)
            .InclusiveBetween(2000, 2100);

        RuleFor(x => x.Quarter)
            .InclusiveBetween(1, 4);

        RuleFor(x => x.RatePerGallon)
            .GreaterThanOrEqualTo(0);

        RuleFor(x => x.SurchargeRatePerGallon)
            .GreaterThanOrEqualTo(0)
            .When(x => x.SurchargeRatePerGallon.HasValue);
    }
}
