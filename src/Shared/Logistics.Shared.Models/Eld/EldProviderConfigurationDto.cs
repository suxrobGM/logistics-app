using Logistics.Domain.Primitives.Enums;

namespace Logistics.Shared.Models;

public record EldProviderConfigurationDto
{
    public Guid Id { get; set; }
    public EldProviderType ProviderType { get; set; }
    public string? ProviderName { get; set; }
    public bool IsActive { get; set; }
    public DateTime? LastSyncedAt { get; set; }
    public bool IsConnected { get; set; }
    public int MappedDriversCount { get; set; }
    public int MappedVehiclesCount { get; set; }

    /// <summary>The account id in the provider's system (TT ELD: the carrier's USDOT number).</summary>
    public string? ExternalAccountId { get; set; }

    /// <summary>Samsara data region (<c>us</c>, <c>eu</c>, <c>ca</c>); null for the vendor default.</summary>
    public string? Region { get; set; }

    /// <summary>Whether a webhook secret is stored, so the UI can tell the tenant webhooks are verifiable.</summary>
    public bool HasWebhookSecret { get; set; }
}
