namespace Logistics.Application.Abstractions.Email.Models;

/// <summary>
/// Model for tenant owner welcome email template.
/// </summary>
public record TenantWelcomeEmailModel
{
    public required string OwnerName { get; init; }
    public required string CompanyName { get; init; }
    public required string Email { get; init; }
    public string? TemporaryPassword { get; init; }
    public required string LoginUrl { get; init; }
}
