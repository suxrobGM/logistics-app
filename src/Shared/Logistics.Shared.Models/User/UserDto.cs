namespace Logistics.Shared.Models;

public class UserDto
{
    public string? Id { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? Email { get; set; }
    public string? PhoneNumber { get; set; }
    public string? TenantId { get; set; }
    public string? TenantName { get; set; }
    public IEnumerable<string> Roles { get; set; } = Array.Empty<string>();
}
