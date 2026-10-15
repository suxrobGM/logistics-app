using Logistics.Domain.Core;
using Logistics.Domain.Primitives.Enums;

namespace Logistics.Domain.Entities;

/// <summary>
/// Per-tenant configuration for load board provider integration.
/// </summary>
public class LoadBoardConfiguration : Entity, ITenantEntity
{
    public required LoadBoardProviderType ProviderType { get; set; }

    /// <summary>
    /// API key or login for the load board provider (DAT: the service account email; encrypted at rest)
    /// </summary>
    [EncryptedSecret]
    public required string ApiKey { get; set; }

    /// <summary>
    /// API secret or password for the load board provider (DAT: the service account password; encrypted at rest)
    /// </summary>
    [EncryptedSecret]
    public string? ApiSecret { get; set; }

    /// <summary>
    /// OAuth access token for providers that use OAuth (encrypted at rest)
    /// </summary>
    [EncryptedSecret]
    public string? AccessToken { get; set; }

    /// <summary>
    /// Credential that renews the access token (DAT: the organization token that mints user tokens; encrypted at rest)
    /// </summary>
    [EncryptedSecret]
    public string? RefreshToken { get; set; }

    /// <summary>
    /// When the current access token expires
    /// </summary>
    public DateTime? TokenExpiresAt { get; set; }

    /// <summary>
    /// The board user the API acts as. DAT issues user tokens per DAT One user, so this is the
    /// email of a DAT One seat on the carrier's account; postings and searches run as that user.
    /// </summary>
    public string? UserEmail { get; set; }

    /// <summary>
    /// Look up the board's market rate for each searched lane (DAT RateView). Off by default:
    /// RateView is a separate DAT subscription, and without it every lookup fails.
    /// </summary>
    public bool MarketRatesEnabled { get; set; }

    /// <summary>
    /// Whether this provider configuration is active
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// When data was last synced from this provider
    /// </summary>
    public DateTime? LastSyncedAt { get; set; }

    /// <summary>
    /// The account/organization ID in the load board provider's system
    /// </summary>
    public string? ExternalAccountId { get; set; }

    /// <summary>
    /// Company DOT number for broker verification
    /// </summary>
    public string? CompanyDotNumber { get; set; }

    /// <summary>
    /// Company MC number for broker verification
    /// </summary>
    public string? CompanyMcNumber { get; set; }
}
