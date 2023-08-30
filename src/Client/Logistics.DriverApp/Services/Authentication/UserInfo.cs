namespace Logistics.DriverApp.Services.Authentication;

public class UserInfo
{
    public string? Id { get; set; }
    public List<string> Roles { get; } = new();
    public List<string> Permissions { get; } = new();
}
