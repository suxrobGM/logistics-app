namespace Logistics.Shared.Models;

/// <summary>
/// Represents a tenant that a portal user has access to.
/// Used for tenant selection in the customer portal.
/// </summary>
public class UserTenantAccessDto
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public string TenantName { get; set; } = string.Empty;
    public string? CompanyName { get; set; }
    public string? CustomerName { get; set; }
    public bool IsActive { get; set; }
    public DateTime? LastAccessedAt { get; set; }
}
