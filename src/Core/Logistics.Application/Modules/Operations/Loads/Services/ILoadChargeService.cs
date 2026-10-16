using Logistics.Domain.Entities;
using Logistics.Shared.Models;

namespace Logistics.Application.Modules.Operations.Loads.Services;

/// <summary>
/// Keeps a load's accessorials and its invoice lines in step. The caller saves.
/// </summary>
public interface ILoadChargeService : IApplicationService
{
    /// <summary>
    /// Makes a tracked load's accessorials match <paramref name="accessorials"/>: rows with a known id
    /// are updated, rows without one are added, rows not listed are removed.
    /// </summary>
    Task ReplaceAccessorialsAsync(Load load, IReadOnlyList<LoadAccessorialDto> accessorials, CancellationToken ct = default);

    /// <summary>
    /// Removes the load's accessorials that <paramref name="owns"/> matches and adds <paramref name="fresh"/>
    /// in their place.
    /// </summary>
    Task ReplaceAccessorialsAsync(
        Load load, Func<LoadAccessorial, bool> owns, IReadOnlyList<LoadAccessorial> fresh, CancellationToken ct = default);

    /// <summary>
    /// Rebuilds the invoice's linehaul, fuel surcharge and accessorial lines from the load and
    /// re-applies tax. Returns false (and changes nothing) when the load has no invoice or it is paid
    /// or cancelled.
    /// </summary>
    Task<bool> SyncInvoiceAsync(Load load, CancellationToken ct = default);

    /// <summary>
    /// Writes the detention a departed stop earned as an accessorial on its load (or removes a stale
    /// one when it earned none) and syncs the invoice.
    /// </summary>
    Task ApplyDetentionAsync(TripStop stop, CancellationToken ct = default);
}
