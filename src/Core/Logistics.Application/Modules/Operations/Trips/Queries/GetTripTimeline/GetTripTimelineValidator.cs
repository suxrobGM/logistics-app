using FluentValidation;

namespace Logistics.Application.Modules.Operations.Trips.Queries;

internal sealed class GetTripTimelineValidator : AbstractValidator<GetTripTimelineQuery>
{
    public GetTripTimelineValidator()
    {
        RuleFor(i => i.TripId).NotEmpty();
    }
}
