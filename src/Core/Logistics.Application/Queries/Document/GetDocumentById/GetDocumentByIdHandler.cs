using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Domain.Primitives.Enums;
using Logistics.Mappings;
using Logistics.Shared.Models;

namespace Logistics.Application.Queries;

internal sealed class GetDocumentByIdHandler : RequestHandler<GetDocumentByIdQuery, Result<DocumentDto>>
{
    private readonly ITenantUnityOfWork _tenantUow;

    public GetDocumentByIdHandler(ITenantUnityOfWork tenantUow)
    {
        _tenantUow = tenantUow;
    }

    protected override async Task<Result<DocumentDto>> HandleValidated(
        GetDocumentByIdQuery req, CancellationToken ct)
    {
        // Query the base type; EF will materialize derived type (TPH)
        var document = await _tenantUow.Repository<Document>()
            .GetAsync(d => d.Id == req.DocumentId, ct);

        if (document is null)
            return Result<DocumentDto>.Fail($"Could not find document with ID '{req.DocumentId}'");

        if (document.Status == DocumentStatus.Deleted)
            return Result<DocumentDto>.Fail("Document has been deleted");

        var dto = document.ToDto();
        return Result<DocumentDto>.Succeed(dto);
    }
}
