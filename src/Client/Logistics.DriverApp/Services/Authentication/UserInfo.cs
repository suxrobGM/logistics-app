namespace Logistics.DriverApp.Services.Authentication;

public class UserInfo
{
    public string? Id { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? CurrentTenantId { get; set; }
    public List<string> TenantIds { get; set; } = new();
    public List<string> Roles { get; } = new();
    public List<string> Permissions { get; } = new();
    public string GetFullName() => $"{FirstName} {LastName}";
}
