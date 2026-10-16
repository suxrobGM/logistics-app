using System.Linq.Expressions;
using Logistics.Application.Abstractions.Accounting;
using Logistics.Domain.Core;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Domain.Primitives.Enums;
using Logistics.Shared.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Logistics.Application.Modules.Integrations.Accounting.Services;

/// <remarks>
/// Saves after each record so a crash loses at most the one in flight (and its request id makes the
/// provider return it again).
/// </remarks>
internal sealed class AccountingMappingService(
    ITenantUnitOfWork tenantUow,
    ILogger<AccountingMappingService> logger) : IAccountingMappingService
{
    public IQueryable<QboEntityMapping> Mappings => tenantUow.Repository<QboEntityMapping>().Query();

    public Expression<Func<T, bool>> NotUpToDate<T>(QboLocalEntityType type) where T : AuditableEntity
    {
        var mappings = Mappings;
        return e => !mappings.Any(m => m.LocalEntityType == type && m.LocalId == e.Id
            && m.SyncStatus != QboSyncStatus.Failed && m.LastSyncedAt >= (e.UpdatedAt ?? e.CreatedAt));
    }

    public async Task LoadAsync(SyncRun run, QboLocalEntityType type, IEnumerable<Guid> localIds, CancellationToken ct)
    {
        var ids = localIds.Distinct().Where(id => !run.Mappings.ContainsKey((type, id))).ToList();
        if (ids.Count == 0)
        {
            return;
        }

        var found = await tenantUow.Repository<QboEntityMapping>()
            .GetListAsync(m => m.LocalEntityType == type && ids.Contains(m.LocalId), ct);
        foreach (var mapping in found)
        {
            run.Mappings[(type, mapping.LocalId)] = mapping;
        }
    }

    public Task PushAsync(SyncRun run, QboLocalEntityType type, Guid localId, string hash,
        Func<QboWriteTarget, Task<QboUpsertResult>> write, CancellationToken ct) =>
        WriteAsync(run, type, localId, hash, write, ct);

    public Task RetireAsync(SyncRun run, QboLocalEntityType type, Guid localId,
        Func<QboWriteTarget, Task<QboUpsertResult>> retire, CancellationToken ct) =>
        run.Mappings.GetValueOrDefault((type, localId))?.QboId is null
            ? Task.CompletedTask
            : WriteAsync(run, type, localId, null, retire, ct);

    public async Task RetireRemovedAsync<T>(SyncRun run, QboLocalEntityType type,
        Func<QboWriteTarget, Task<QboUpsertResult>> retire, CancellationToken ct)
        where T : class, IEntity<Guid>, ITenantEntity
    {
        var rows = tenantUow.Repository<T>().Query();
        var orphaned = await Mappings
            .Where(m => m.LocalEntityType == type && m.QboId != null
                        && m.SyncStatus != QboSyncStatus.Retired
                        && !rows.Any(r => r.Id == m.LocalId))
            .Take(SyncRun.BatchSize)
            .ToListAsync(ct);

        foreach (var mapping in orphaned)
        {
            run.Mappings.TryAdd((mapping.LocalEntityType, mapping.LocalId), mapping);
            await RetireAsync(run, type, mapping.LocalId, retire, ct);
        }
    }

    public async Task RecordFailureAsync(
        SyncRun run, QboLocalEntityType type, Guid localId, string message, CancellationToken ct)
    {
        var mapping = run.Mappings.GetValueOrDefault((type, localId)) ?? await AddMappingAsync(run, type, localId, ct);
        Fail(mapping, message);
        await tenantUow.SaveChangesAsync(ct);
    }

    /// <summary>A null <paramref name="hash"/> marks the record retired rather than synced.</summary>
    private async Task WriteAsync(SyncRun run, QboLocalEntityType type, Guid localId, string? hash,
        Func<QboWriteTarget, Task<QboUpsertResult>> write, CancellationToken ct)
    {
        var mapping = run.Mappings.GetValueOrDefault((type, localId));
        var status = hash is null ? QboSyncStatus.Retired : QboSyncStatus.Synced;

        if (mapping is not null && mapping.SyncStatus == status && mapping.LastSyncedHash == hash)
        {
            // Touched but unchanged: mark it current so the selection stops loading it (saved at the end of the run).
            mapping.LastSyncedAt = run.StartedAt;
            return;
        }

        mapping ??= await AddMappingAsync(run, type, localId, ct);

        try
        {
            var result = await write(new QboWriteTarget(mapping.QboId, mapping.QboSyncToken,
                AccountingPayloads.RequestId(localId, hash ?? nameof(QboSyncStatus.Retired))));
            mapping.QboId = result.QboId;
            mapping.QboSyncToken = result.SyncToken;
            mapping.LastSyncedHash = hash;
            mapping.LastSyncedAt = run.StartedAt;
            mapping.SyncStatus = status;
            mapping.LastError = null;
        }
        catch (Exception ex) when (ex is not AccountingReconnectRequiredException and not OperationCanceledException)
        {
            Fail(mapping, ex.Message);
            logger.LogWarning(ex, "Failed to push {Type} {LocalId} to QuickBooks", type, localId);
        }

        await tenantUow.SaveChangesAsync(ct);
    }

    private async Task<QboEntityMapping> AddMappingAsync(
        SyncRun run, QboLocalEntityType type, Guid localId, CancellationToken ct)
    {
        var mapping = new QboEntityMapping { LocalId = localId, LocalEntityType = type };
        await tenantUow.Repository<QboEntityMapping>().AddAsync(mapping, ct);
        run.Mappings[(type, localId)] = mapping;
        return mapping;
    }

    private static void Fail(QboEntityMapping mapping, string message)
    {
        mapping.SyncStatus = QboSyncStatus.Failed;
        mapping.LastError = message.Length > 2000 ? message[..2000] : message;
    }
}
