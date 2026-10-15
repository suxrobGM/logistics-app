using Logistics.Application.Modules.Platform.DataImport.Constants;
using Logistics.Mediator;
using Logistics.Shared.Models;

namespace Logistics.Application.Modules.Platform.DataImport.Queries;

internal sealed class GetImportPresetsHandler
    : IRequestHandler<GetImportPresetsQuery, Result<IReadOnlyList<ImportPresetDto>>>
{
    public Task<Result<IReadOnlyList<ImportPresetDto>>> Handle(GetImportPresetsQuery req, CancellationToken ct)
    {
        IReadOnlyList<ImportPresetDto> presets =
            [.. ImportPresets.All.Select(p => new ImportPresetDto { Id = p.Id, Name = p.Name })];
        return Task.FromResult(Result<IReadOnlyList<ImportPresetDto>>.Ok(presets));
    }
}
