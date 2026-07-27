namespace Logistics.DbMigrator.Models;

public record UserData
{
    public required string Email { get; set; }
    public required string FirstName { get; set; }
    public required string LastName { get; set; }
    public required string Password { get; set; }

    /// <summary>
    /// Tenant role this user should be given, as a <c>TenantRoles</c> constant
    /// (<c>tenant.owner</c>, <c>tenant.manager</c>, <c>tenant.dispatcher</c>, <c>tenant.driver</c>).
    /// When no user in a seed set sets this, EmployeeSeeder falls back to its positional split.
    /// </summary>
    public string? Role { get; set; }
}
