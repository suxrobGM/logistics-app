using FluentValidation;

namespace Logistics.Application.Queries;

internal sealed class GetTripValidator : AbstractValidator<GetTripQuery>
{
    public GetTripValidator()
    {
        RuleFor(i => i.TripId).NotEmpty();
    }
}
