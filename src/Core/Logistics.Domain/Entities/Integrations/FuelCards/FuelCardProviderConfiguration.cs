using Logistics.Domain.Core;
using Logistics.Domain.Primitives.Enums;

namespace Logistics.Domain.Entities;

public class FuelCardProviderConfiguration : Entity, ITenantEntity
{
    public required FuelCardProviderType ProviderType { get; set; }

    /// <summary>
    /// API key or client ID for the fuel card provider (encrypted at rest)
    /// </summary>
    [EncryptedSecret]
    public required string ApiKey { get; set; }

    /// <summary>
    /// API secret or client secret for the fuel card provider (encrypted at rest)
    /// </summary>
    [EncryptedSecret]
    public string? ApiSecret { get; set; }

    /// <summary>
    /// OAuth access token for providers that use OAuth (encrypted at rest)
    /// </summary>
    [EncryptedSecret]
    public string? AccessToken { get; set; }

    /// <summary>
    /// OAuth refresh token for token renewal (encrypted at rest)
    /// </summary>
    [EncryptedSecret]
    public string? RefreshToken { get; set; }

    /// <summary>
    /// When the current access token expires
    /// </summary>
    public DateTime? TokenExpiresAt { get; set; }

    /// <summary>
    /// Whether this provider configuration is active
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// When transactions were last synced from this provider
    /// </summary>
    public DateTime? LastSyncedAt { get; set; }

    /// <summary>
    /// The carrier's account ID in the fuel card provider's system
    /// </summary>
    public string? ExternalAccountId { get; set; }
}
