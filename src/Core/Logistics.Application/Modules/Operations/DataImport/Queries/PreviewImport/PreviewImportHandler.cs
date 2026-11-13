using Logistics.Application.Abstractions.Documents;
using Logistics.Application.Modules.Operations.DataImport.Constants;
using Logistics.Application.Modules.Operations.DataImport.Services;
using Logistics.Mediator;
using Logistics.Shared.Models;

namespace Logistics.Application.Modules.Operations.DataImport.Queries;

internal sealed class PreviewImportHandler(ISpreadsheetReader spreadsheetReader)
    : IRequestHandler<PreviewImportQuery, Result<ImportPreviewDto>>
{
    private const int PreviewRowCount = 10;

    public Task<Result<ImportPreviewDto>> Handle(PreviewImportQuery req, CancellationToken ct)
    {
        var table = ImportFileReader.Read(spreadsheetReader, req);
        if (!table.IsSuccess)
        {
            return Task.FromResult(Result<ImportPreviewDto>.Fail(table));
        }

        var headers = table.Value!.Headers;
        var preview = new ImportPreviewDto
        {
            EntityType = req.EntityType,
            Headers = headers,
            Fields =
            [
                .. ImportFieldCatalog.For(req.EntityType)
                    .Select(f => new ImportFieldDto { Key = f.Key, Label = f.Label, Required = f.Required })
            ],
            Mapping = ImportHeaderMapper.Suggest(headers, req.EntityType),
            PreviewRows = [.. table.Value.Rows.Take(PreviewRowCount).Select(r => r.Cells)],
            TotalRows = table.Value.Rows.Count
        };

        return Task.FromResult(Result<ImportPreviewDto>.Ok(preview));
    }
}
