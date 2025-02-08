using Logistics.Shared.Models;
using Logistics.Shared.Models;
using MediatR;

namespace Logistics.Application.Queries;

public class GetUserJoinedOrganizationsQuery : IRequest<Result<OrganizationDto[]>>
{
    public required string UserId { get; set; }
}
