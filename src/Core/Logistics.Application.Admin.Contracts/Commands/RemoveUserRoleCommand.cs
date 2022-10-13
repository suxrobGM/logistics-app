namespace Logistics.Application.Admin.Commands;

public class RemoveUserRoleCommand : RequestBase<ResponseResult>
{
    public string? UserId { get; set; }
    public string? Role { get; set; }
}