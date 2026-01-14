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
}

public record CreateEldProviderConfigurationDto
{
    public EldProviderType ProviderType { get; set; }
    public required string ApiKey { get; set; }
    public string? ApiSecret { get; set; }
    public string? WebhookSecret { get; set; }
}

public record UpdateEldProviderConfigurationDto
{
    public Guid Id { get; set; }
    public string? ApiKey { get; set; }
    public string? ApiSecret { get; set; }
    public string? WebhookSecret { get; set; }
    public bool? IsActive { get; set; }
}
