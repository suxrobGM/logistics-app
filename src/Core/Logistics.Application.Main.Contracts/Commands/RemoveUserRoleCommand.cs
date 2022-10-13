namespace Logistics.Application.Main.Commands;

public class RemoveUserRoleCommand : RequestBase<ResponseResult>
{
    public string? UserId { get; set; }
    public string? Role { get; set; }
}