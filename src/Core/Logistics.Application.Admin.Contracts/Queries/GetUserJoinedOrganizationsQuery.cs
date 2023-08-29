namespace Logistics.Application.Admin.Queries;

public class GetUserJoinedOrganizationsQuery : RequestBase<ResponseResult<UserOrganizationsDto>>
{
    public required string UserId { get; set; }
}
