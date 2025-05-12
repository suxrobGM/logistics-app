namespace Logistics.Shared.Models;

public record UserDto
{
    public Guid? Id { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? Email { get; set; }
    public string? PhoneNumber { get; set; }
    public Guid? TenantId { get; set; }
    public string? TenantName { get; set; }
    public IEnumerable<string> Roles { get; set; } = [];
}
