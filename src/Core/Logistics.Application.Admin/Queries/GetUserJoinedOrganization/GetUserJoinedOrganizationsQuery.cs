using Logistics.Application.Common;
using Logistics.Models;
using Logistics.Shared;

namespace Logistics.Application.Admin.Queries;

public class GetUserJoinedOrganizationsQuery : Request<ResponseResult<OrganizationDto[]>>
{
    public required string UserId { get; set; }
}
