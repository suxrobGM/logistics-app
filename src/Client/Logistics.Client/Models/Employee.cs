namespace Logistics.Client.Models;

public record Employee
{
    public string? Id { get; set; }
    public string? UserName { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? Email { get; set; }
    public string? PhoneNumber { get; set; }
    public DateTime JoinedDate { get; set; } = DateTime.UtcNow;
    public List<TenantRole> Roles { get; set; } = new();
}
