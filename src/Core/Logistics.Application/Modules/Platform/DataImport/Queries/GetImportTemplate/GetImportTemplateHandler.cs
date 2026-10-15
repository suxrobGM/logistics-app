using Logistics.Application.Modules.Platform.DataImport.Services;
using Logistics.Mediator;
using Logistics.Shared.Models;

namespace Logistics.Application.Modules.Platform.DataImport.Queries;

internal sealed class GetImportTemplateHandler : IRequestHandler<GetImportTemplateQuery, Result<string>>
{
    public Task<Result<string>> Handle(GetImportTemplateQuery req, CancellationToken ct) =>
        Task.FromResult(Result<string>.Ok(ImportRowMapper.BuildTemplate(req.EntityType)));
}
