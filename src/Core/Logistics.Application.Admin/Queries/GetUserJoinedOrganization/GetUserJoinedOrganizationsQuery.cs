using Logistics.Models;

namespace Logistics.Application.Admin.Queries;

public class GetUserJoinedOrganizationsQuery : Request<ResponseResult<OrganizationDto[]>>
{
    public required string UserId { get; set; }
}
