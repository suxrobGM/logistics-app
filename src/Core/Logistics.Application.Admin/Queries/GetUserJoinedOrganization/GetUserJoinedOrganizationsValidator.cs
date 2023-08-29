using FluentValidation;

namespace Logistics.Application.Admin.Queries;

internal class GetUserJoinedOrganizationsValidator : AbstractValidator<GetUserJoinedOrganizationsQuery>
{
    public GetUserJoinedOrganizationsValidator()
    {
        RuleFor(i => i.UserId).NotEmpty();
    }
}
