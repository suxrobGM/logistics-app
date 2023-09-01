namespace Logistics.Application.Admin.Commands;

public class RemoveUserRoleCommand : Request<ResponseResult>
{
    public string? UserId { get; set; }
    public string? Role { get; set; }
}