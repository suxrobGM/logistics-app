namespace Logistics.Application.Contracts.Commands;

public class RemoveUserRoleCommand : RequestBase<DataResult>
{
    public string? UserId { get; set; }
    public string? Role { get; set; }
}