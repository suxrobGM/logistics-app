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

    /// <summary>
    /// The login, for providers whose login is not itself a secret (DAT: the service account
    /// email), so the edit dialog can show which account is connected. API keys and passwords
    /// are never returned.
    /// </summary>
    public string? AccountLogin { get; set; }

    /// <summary>
    /// The board user the API acts as (DAT: the DAT One user's email).
    /// </summary>
    public string? UserEmail { get; set; }

    public bool MarketRatesEnabled { get; set; }
}
