using Logistics.Domain.Primitives.Enums;

namespace Logistics.Shared.Models;

/// <summary>
/// Invitation details for listing and display.
/// </summary>
public record InvitationDto
{
    public Guid Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public Guid TenantId { get; set; }
    public string TenantName { get; set; } = string.Empty;
    public InvitationType Type { get; set; }
    public string TenantRole { get; set; } = string.Empty;
    public string TenantRoleDisplayName { get; set; } = string.Empty;
    public Guid? CustomerId { get; set; }
    public string? CustomerName { get; set; }
    public DateTime ExpiresAt { get; set; }
    public InvitationStatus Status { get; set; }
    public DateTime CreatedAt { get; set; }
    public string InvitedByName { get; set; } = string.Empty;
    public int SendCount { get; set; }
    public DateTime? LastSentAt { get; set; }
    public bool IsExpired { get; set; }
}
