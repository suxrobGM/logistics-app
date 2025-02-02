using Logistics.Shared.Models;
using Logistics.Shared;
using MediatR;

namespace Logistics.Application.Queries;

public class GetUserJoinedOrganizationsQuery : IRequest<Result<OrganizationDto[]>>
{
    public required string UserId { get; set; }
}
