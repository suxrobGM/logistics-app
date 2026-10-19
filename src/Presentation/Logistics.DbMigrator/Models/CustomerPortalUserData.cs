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
    /// The customer this user signs in for; a portal user is active for one customer per tenant.
    /// </summary>
    public required string CustomerName { get; set; }
}
