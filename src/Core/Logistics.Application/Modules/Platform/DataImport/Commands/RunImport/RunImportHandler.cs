using Logistics.Application.Abstractions.Documents;
using Logistics.Application.Modules.Platform.DataImport.Constants;
using Logistics.Application.Modules.Platform.DataImport.Importers;
using Logistics.Application.Modules.Platform.DataImport.Services;
using Logistics.Domain.Persistence;
using Logistics.Domain.Primitives.Enums;
using Logistics.Mediator;
using Logistics.Shared.Models;

namespace Logistics.Application.Modules.Platform.DataImport.Commands;

internal sealed class RunImportHandler(
    ITenantUnitOfWork tenantUow,
    ISpreadsheetReader spreadsheetReader,
    IEnumerable<IEntityImporter> importers)
    : IRequestHandler<RunImportCommand, Result<ImportResultDto>>
{
    public async Task<Result<ImportResultDto>> Handle(RunImportCommand req, CancellationToken ct)
    {
        var table = ImportFileReader.Read(spreadsheetReader, req);
        if (!table.IsSuccess)
        {
            return Result<ImportResultDto>.Fail(table.Error!);
        }

        var mappedKeys = req.Mapping.Where(m => m.FieldKey is not null).Select(m => m.FieldKey!).ToHashSet();
        var unmapped = ImportFieldCatalog.For(req.EntityType)
            .Where(f => f.Required && !mappedKeys.Contains(f.Key))
            .ToList();
        if (unmapped.Count > 0)
        {
            return Result<ImportResultDto>.Fail($"Map a column to: {string.Join(", ", unmapped.Select(f => f.Label))}.");
        }

        var importer = importers.Single(i => i.EntityType == req.EntityType);
        var rows = ImportRowMapper.ToRows(table.Value!, req.EntityType, req.Mapping);
        var options = ImportOptionsFactory.For(
            tenantUow.GetCurrentTenant(), !req.DryRun, req.UpdateExisting, req.SendInvitations);

        var results = await importer.ImportAsync(rows, options, ct);

        return Result<ImportResultDto>.Ok(new ImportResultDto
        {
            EntityType = req.EntityType,
            Committed = !req.DryRun,
            TotalRows = results.Count,
            Created = results.Count(r => r.Action == ImportRowAction.Create),
            Updated = results.Count(r => r.Action == ImportRowAction.Update),
            Skipped = results.Count(r => r.Action == ImportRowAction.Skip),
            Failed = results.Count(r => r.Action == ImportRowAction.Error),
            Rows = results
        });
    }
}
