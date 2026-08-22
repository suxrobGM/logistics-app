using FluentValidation;

namespace Logistics.Application.Modules.Integrations.Negotiation.Commands;

internal abstract class LaneRateFloorValidatorBase<T> : AbstractValidator<T> where T : ILaneRateFloorFields
{
    protected LaneRateFloorValidatorBase()
    {
        RuleFor(x => x.OriginCountry)
            .NotEmpty()
            .Length(2);

        RuleFor(x => x.OriginState)
            .Length(2)
            .When(x => x.OriginState is not null);

        RuleFor(x => x.DestinationCountry)
            .NotEmpty()
            .Length(2);

        RuleFor(x => x.DestinationState)
            .Length(2)
            .When(x => x.DestinationState is not null);

        RuleFor(x => x.MinRatePerMile)
            .GreaterThan(0);

        RuleFor(x => x.MinTotalRateAmount)
            .GreaterThan(0)
            .When(x => x.MinTotalRateAmount.HasValue);
    }
}
