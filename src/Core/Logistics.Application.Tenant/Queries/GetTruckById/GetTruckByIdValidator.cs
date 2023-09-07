using FluentValidation;

namespace Logistics.Application.Tenant.Queries;

internal sealed class GetTruckByIdValidator : AbstractValidator<GetTruckByIdQuery>
{
    public GetTruckByIdValidator()
    {
        RuleFor(i => i.Id).NotEmpty();
    }
}
