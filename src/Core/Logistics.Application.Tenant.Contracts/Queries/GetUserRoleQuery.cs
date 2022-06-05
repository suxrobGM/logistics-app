namespace Logistics.Application.Contracts.Queries;

public class GetUserRoleQuery : RequestBase<DataResult<UserRoleDto>>
{
    public string? UserId { get; set; }
}
