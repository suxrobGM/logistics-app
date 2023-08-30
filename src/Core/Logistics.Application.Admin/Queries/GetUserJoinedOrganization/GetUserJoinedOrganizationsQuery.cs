using Logistics.Models;

namespace Logistics.Application.Admin.Queries;

public class GetUserJoinedOrganizationsQuery : RequestBase<ResponseResult<OrganizationDto[]>>
{
    public required string UserId { get; set; }
}
