using FluentValidation;

namespace Logistics.Application.Queries;

internal sealed class GetTripTimelineValidator : AbstractValidator<GetTripTimelineQuery>
{
    public GetTripTimelineValidator()
    {
        RuleFor(i => i.TripId).NotEmpty();
    }
}
