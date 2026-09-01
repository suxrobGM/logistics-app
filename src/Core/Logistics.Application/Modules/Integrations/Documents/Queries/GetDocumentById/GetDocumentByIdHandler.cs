using Logistics.Application.Abstractions;
using Logistics.Application.Modules.Integrations.Documents.Services;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Domain.Primitives.Enums;
using Logistics.Mappings;
using Logistics.Shared.Models;

namespace Logistics.Application.Modules.Integrations.Documents.Queries;

internal sealed class GetDocumentByIdHandler(
    ITenantUnitOfWork tenantUow,
    IDocumentAccessService documentAccess)
    : IAppRequestHandler<GetDocumentByIdQuery, Result<DocumentDto>>
{
    public async Task<Result<DocumentDto>> Handle(GetDocumentByIdQuery req, CancellationToken ct)
    {
        // Query the base type; EF will materialize derived type (TPH)
        var document = await tenantUow.Repository<Document>()
            .GetAsync(d => d.Id == req.DocumentId, ct);

        if (document is null)
        {
            return Result<DocumentDto>.Fail($"Could not find document with ID '{req.DocumentId}'");
        }

        if (document.Status == DocumentStatus.Deleted)
        {
            return Result<DocumentDto>.Fail("Document has been deleted");
        }

        var caller = await documentAccess.ResolveCallerAsync(ct);
        if (caller is null || !await documentAccess.CanAccessAsync(caller, document, ct))
        {
            return Result<DocumentDto>.Fail("Document not found or access denied.");
        }

        var dto = document.ToDto();
        return Result<DocumentDto>.Ok(dto);
    }
}
