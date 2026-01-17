namespace Logistics.Application.Services;

/// <summary>
/// Model for invitation email templates.
/// </summary>
public record InvitationEmailModel
{
    public required string InvitedByName { get; init; }
    public required string CompanyName { get; init; }
    public required string TypeLabel { get; init; }
    public required string RoleDisplayName { get; init; }
    public string? PersonalMessage { get; init; }
    public required string AcceptUrl { get; init; }
    public required string ExpiresAt { get; init; }
}
