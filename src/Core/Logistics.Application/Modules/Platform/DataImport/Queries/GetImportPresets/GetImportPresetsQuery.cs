using Logistics.Shared.Models;

namespace Logistics.Application.Modules.Platform.DataImport.Queries;

public class GetImportPresetsQuery : IQuery<Result<IReadOnlyList<ImportPresetDto>>>;
