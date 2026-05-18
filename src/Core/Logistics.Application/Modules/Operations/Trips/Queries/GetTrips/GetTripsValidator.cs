using FluentValidation;
using Logistics.Application.Modules.IdentityAccess.Tenants.Queries;

namespace Logistics.Application.Modules.Operations.Trips.Queries;

internal sealed class GetTripsValidator : AbstractValidator<GetTenantsQuery>
{
    public GetTripsValidator()
    {
        RuleFor(i => i.Page)
            .GreaterThanOrEqualTo(0);

        RuleFor(i => i.PageSize)
            .GreaterThanOrEqualTo(1);
    }
}
