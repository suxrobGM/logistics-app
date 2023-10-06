using FluentValidation;

namespace Logistics.Application.Tenant.Queries;

internal sealed class GetTruckValidator : AbstractValidator<GetTruckQuery>
{
    public GetTruckValidator()
    {
        RuleFor(i => i.TruckOrDriverId)
            .NotEmpty()
            .WithMessage("Specify either TruckId or driver UserId.");
    }
}
