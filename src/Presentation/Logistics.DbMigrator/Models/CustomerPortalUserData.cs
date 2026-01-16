namespace Logistics.DbMigrator.Models;

/// <summary>
/// Configuration data for a customer portal user.
/// </summary>
public record CustomerPortalUserData
{
    public required string Email { get; set; }
    public required string FirstName { get; set; }
    public required string LastName { get; set; }
    public required string Password { get; set; }

    /// <summary>
    /// Names of customers this user should be associated with.
    /// Supports multi-tenant scenarios where a user can access multiple companies.
    /// </summary>
    public required string[] CustomerNames { get; set; }
}
