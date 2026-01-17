namespace Logistics.Shared.Models;

/// <summary>
/// Result of accepting an invitation.
/// </summary>
public record AcceptInvitationResult
{
    public Guid UserId { get; set; }
    public string Email { get; set; } = string.Empty;
    public string TenantName { get; set; } = string.Empty;
    public string RoleDisplayName { get; set; } = string.Empty;
}
