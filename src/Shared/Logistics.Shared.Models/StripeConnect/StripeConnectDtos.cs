namespace Logistics.Shared.Models;

/// <summary>
/// Result returned when creating a new Stripe Connect account.
/// </summary>
public record CreateConnectAccountDto
{
    public required string AccountId { get; init; }
}

/// <summary>
/// Result returned when requesting an onboarding link.
/// </summary>
public record OnboardingLinkDto
{
    public required string Url { get; init; }
    public DateTime ExpiresAt { get; init; }
}

/// <summary>
/// Result returned when requesting a Stripe Express dashboard link.
/// </summary>
public record DashboardLinkDto
{
    public required string Url { get; init; }
}
