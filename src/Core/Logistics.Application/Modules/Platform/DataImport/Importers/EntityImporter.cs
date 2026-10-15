using Logistics.Application.Modules.Platform.DataImport.Services;
using Logistics.Domain.Primitives.Enums;
using Logistics.Shared.Models;

namespace Logistics.Application.Modules.Platform.DataImport.Importers;

/// <summary>
///     The parse, de-duplicate, look up, decide and write pipeline every entity shares. Subclasses
///     own only what differs: reading a row, finding what exists, and writing.
/// </summary>
internal abstract class EntityImporter<TDraft> : IEntityImporter where TDraft : ImportDraft
{
    public abstract ImportEntityType EntityType { get; }

    public async Task<IReadOnlyList<ImportRowResultDto>> ImportAsync(
        IReadOnlyList<ImportRow> rows, ImportOptions options, CancellationToken ct)
    {
        var drafts = rows.Select(r => Parse(r, options)).ToList();

        var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var draft in drafts.Where(d => d.Errors.Count == 0 && d.Key.Length > 0))
        {
            if (!seen.Add(draft.Key))
            {
                draft.Errors.Add($"Duplicate of an earlier row with '{draft.Key}' in this file.");
            }
        }

        await ResolveAsync([.. drafts.Where(d => d.Errors.Count == 0)], options, ct);

        foreach (var draft in drafts)
        {
            draft.Action = Decide(draft, options);
        }

        if (options.Commit)
        {
            var writable = drafts.Where(d => d.Action is ImportRowAction.Create or ImportRowAction.Update).ToList();
            if (writable.Count > 0)
            {
                await WriteAsync(writable, options, ct);
            }

            foreach (var draft in writable.Where(d => d.Errors.Count > 0))
            {
                draft.Action = ImportRowAction.Error;
            }
        }

        return [.. drafts.Select(ToResult)];
    }

    /// <summary>
    ///     Reads one row. Missing required values and unparseable cells go to <see cref="ImportDraft.Errors"/>.
    /// </summary>
    protected abstract TDraft Parse(ImportRow row, ImportOptions options);

    /// <summary>
    ///     Batch lookups for the valid drafts: sets <see cref="ImportDraft.Exists"/> and records unknown
    ///     references (customer, truck, driver) as errors.
    /// </summary>
    protected abstract Task ResolveAsync(IReadOnlyList<TDraft> drafts, ImportOptions options, CancellationToken ct);

    /// <summary>
    ///     Creates or updates the drafts and saves. A row that fails here gets an error, not an exception.
    /// </summary>
    protected abstract Task WriteAsync(IReadOnlyList<TDraft> drafts, ImportOptions options, CancellationToken ct);

    private static ImportRowAction Decide(TDraft draft, ImportOptions options)
    {
        if (draft.Errors.Count > 0)
        {
            return ImportRowAction.Error;
        }

        if (!draft.Exists)
        {
            return ImportRowAction.Create;
        }

        if (options.UpdateExisting)
        {
            return ImportRowAction.Update;
        }

        draft.Notices.Add($"'{draft.Key}' already exists; turn on \"update existing\" to overwrite it.");
        return ImportRowAction.Skip;
    }

    private static ImportRowResultDto ToResult(TDraft draft) => new()
    {
        RowNumber = draft.Row.RowNumber,
        Action = draft.Action,
        Key = draft.Key.Length > 0 ? draft.Key : null,
        Messages = [.. draft.Errors, .. draft.Notices]
    };
}
