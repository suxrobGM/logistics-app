using FluentValidation;

namespace Logistics.Application.Queries;

internal sealed class GetUserJoinedOrganizationsValidator : AbstractValidator<GetUserJoinedOrganizationsQuery>
{
    public GetUserJoinedOrganizationsValidator()
    {
        RuleFor(i => i.UserId).NotEmpty();
    }
}
