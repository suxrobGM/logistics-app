using FluentValidation;

namespace Logistics.Application.Tenant.Queries;

internal sealed class GetTruckByDriverValidator : AbstractValidator<GetTruckByDriverQuery>
{
    public GetTruckByDriverValidator()
    {
        RuleFor(i => i.UserId).NotEmpty();
    }
}
