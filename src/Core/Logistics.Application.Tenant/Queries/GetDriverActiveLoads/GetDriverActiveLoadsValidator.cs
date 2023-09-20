using FluentValidation;

namespace Logistics.Application.Tenant.Queries;

internal sealed class GetDriverActiveLoadsValidator : AbstractValidator<GetDriverActiveLoadsQuery>
{
    public GetDriverActiveLoadsValidator()
    {
        RuleFor(i => i.UserId).NotEmpty();
    }
}
