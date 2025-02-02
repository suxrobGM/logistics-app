using FluentValidation;

namespace Logistics.Application.Queries;

internal sealed class GetTruckValidator : AbstractValidator<GetTruckQuery>
{
    public GetTruckValidator()
    {
        RuleFor(i => i.TruckOrDriverId)
            .NotEmpty()
            .WithMessage("Specify either TruckId or driver UserId.");
    }
}
