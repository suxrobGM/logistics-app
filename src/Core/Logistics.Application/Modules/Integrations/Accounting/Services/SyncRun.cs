using Logistics.Application.Abstractions.Accounting;
using Logistics.Domain.Entities;
using Logistics.Domain.Primitives.Enums;

namespace Logistics.Application.Modules.Integrations.Accounting.Services;

/// <summary>One accounting sync pass: the provider, its settings, and the entity mappings loaded so far.</summary>
internal sealed class SyncRun(IAccountingProviderService provider, AccountingProviderConfiguration config)
{
    /// <summary>Records per type per run, so a first sync of a large history spreads over several runs.</summary>
    public const int BatchSize = 500;

    public IAccountingProviderService Provider { get; } = provider;
    public AccountingProviderConfiguration Config { get; } = config;
    public DateTime StartedAt { get; } = DateTime.UtcNow;
    public DateTime SyncFrom { get; } = config.SyncFromDate ?? DateTime.UnixEpoch;
    public Dictionary<(QboLocalEntityType, Guid), QboEntityMapping> Mappings { get; } = [];

    /// <summary>The provider id of a record pushed and not retired since, else null.</summary>
    public string? SyncedQboId(QboLocalEntityType type, Guid localId) =>
        Mappings.TryGetValue((type, localId), out var m) && m.SyncStatus != QboSyncStatus.Retired
            ? m.QboId
            : null;
}
