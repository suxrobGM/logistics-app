namespace Logistics.DriverApp.Authentication;

public class UserIdentity
{
    public string? Id { get; set; }
    public string? UserName { get; set; }
    public IList<string> Roles { get; set; } = new List<string>();
    public IList<string> Permissions { get; set; } = new List<string>();
}
