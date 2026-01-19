using Logistics.Domain.Primitives.Enums;

namespace Logistics.Shared.Models;

public record LoadBoardConfigurationDto
{
    public Guid Id { get; set; }
    public LoadBoardProviderType ProviderType { get; set; }
    public string? ProviderName { get; set; }
    public bool IsActive { get; set; }
    public DateTime? LastSyncedAt { get; set; }
    public bool IsConnected { get; set; }
    public int ActiveListingsCount { get; set; }
    public int PostedTrucksCount { get; set; }
    public string? CompanyDotNumber { get; set; }
    public string? CompanyMcNumber { get; set; }
}

public record CreateLoadBoardConfigurationDto
{
    public LoadBoardProviderType ProviderType { get; set; }
    public required string ApiKey { get; set; }
    public string? ApiSecret { get; set; }
    public string? WebhookSecret { get; set; }
    public string? CompanyDotNumber { get; set; }
    public string? CompanyMcNumber { get; set; }
}

public record UpdateLoadBoardConfigurationDto
{
    public Guid Id { get; set; }
    public string? ApiKey { get; set; }
    public string? ApiSecret { get; set; }
    public string? WebhookSecret { get; set; }
    public bool? IsActive { get; set; }
    public string? CompanyDotNumber { get; set; }
    public string? CompanyMcNumber { get; set; }
}
