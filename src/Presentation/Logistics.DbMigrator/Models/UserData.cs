namespace Logistics.DbMigrator.Models;

public record UserData
{
    public required string Email { get; set; }
    public required string FirstName { get; set; }
    public required string LastName { get; set; }
    public required string Password { get; set; }

    /// <summary>A <c>TenantRoles</c> constant (e.g. <c>tenant.driver</c>); when unset everywhere, EmployeeSeeder splits positionally.</summary>
    public string? Role { get; set; }
}
