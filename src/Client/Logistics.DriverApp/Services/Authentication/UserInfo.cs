namespace Logistics.DriverApp.Services.Authentication;

public class UserInfo
{
    public string? Id { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? TenantId { get; set; } 
    public List<string> Roles { get; } = [];
    public List<string> Permissions { get; } = [];
    public string GetFullName() => $"{FirstName} {LastName}";
}
