using Logistics.Domain.Primitives.Enums;

namespace Logistics.Shared.Models;

public record FuelCardProviderConfigurationDto
{
    public Guid Id { get; set; }
    public FuelCardProviderType ProviderType { get; set; }
    public string? ProviderName { get; set; }
    public bool IsActive { get; set; }
    public bool IsConnected { get; set; }
    public DateTime? LastSyncedAt { get; set; }
    public string? ExternalAccountId { get; set; }

    /// <summary>
    /// Transactions from this provider awaiting truck assignment.
    /// </summary>
    public int PendingTransactionsCount { get; set; }
}
