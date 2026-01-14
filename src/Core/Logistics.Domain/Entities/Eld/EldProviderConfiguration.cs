using Logistics.Domain.Core;
using Logistics.Domain.Primitives.Enums;

namespace Logistics.Domain.Entities;

public class EldProviderConfiguration : Entity, ITenantEntity
{
    public required EldProviderType ProviderType { get; set; }

    /// <summary>
    /// API key or client ID for the ELD provider (encrypted at rest)
    /// </summary>
    public required string ApiKey { get; set; }

    /// <summary>
    /// API secret or client secret for the ELD provider (encrypted at rest)
    /// </summary>
    public string? ApiSecret { get; set; }

    /// <summary>
    /// OAuth access token for providers that use OAuth (encrypted at rest)
    /// </summary>
    public string? AccessToken { get; set; }

    /// <summary>
    /// OAuth refresh token for token renewal (encrypted at rest)
    /// </summary>
    public string? RefreshToken { get; set; }

    /// <summary>
    /// When the current access token expires
    /// </summary>
    public DateTime? TokenExpiresAt { get; set; }

    /// <summary>
    /// Secret for validating incoming webhooks from the provider
    /// </summary>
    public string? WebhookSecret { get; set; }

    /// <summary>
    /// Whether this provider configuration is active
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// When data was last synced from this provider
    /// </summary>
    public DateTime? LastSyncedAt { get; set; }

    /// <summary>
    /// The account/organization ID in the ELD provider's system
    /// </summary>
    public string? ExternalAccountId { get; set; }
}
