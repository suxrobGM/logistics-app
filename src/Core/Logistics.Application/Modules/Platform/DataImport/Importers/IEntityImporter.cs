using Logistics.Application.Modules.Platform.DataImport.Services;
using Logistics.Domain.Primitives.Enums;
using Logistics.Shared.Models;

namespace Logistics.Application.Modules.Platform.DataImport.Importers;

/// <summary>
///     Validates and (when <see cref="ImportOptions.Commit"/>) writes the rows of one entity type.
///     Returns one result per input row, in input order.
/// </summary>
internal interface IEntityImporter : IApplicationService
{
    ImportEntityType EntityType { get; }

    Task<IReadOnlyList<ImportRowResultDto>> ImportAsync(
        IReadOnlyList<ImportRow> rows, ImportOptions options, CancellationToken ct);
}
