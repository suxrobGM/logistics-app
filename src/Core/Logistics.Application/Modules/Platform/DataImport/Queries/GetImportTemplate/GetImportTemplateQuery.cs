using Logistics.Domain.Primitives.Enums;
using Logistics.Shared.Models;

namespace Logistics.Application.Modules.Platform.DataImport.Queries;

/// <summary>
///     A blank CSV with one header per field, which auto-maps back on upload.
/// </summary>
public class GetImportTemplateQuery : IQuery<Result<string>>
{
    public ImportEntityType EntityType { get; set; }
}
