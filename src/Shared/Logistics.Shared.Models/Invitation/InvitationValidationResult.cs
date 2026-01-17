namespace Logistics.Shared.Models;

/// <summary>
/// Result of validating an invitation token.
/// </summary>
public record InvitationValidationResult
{
    public bool IsValid { get; set; }
    public string? Email { get; set; }
    public string? TenantName { get; set; }
    public string? RoleDisplayName { get; set; }
    public string? ErrorMessage { get; set; }
    public bool UserExists { get; set; }
}
