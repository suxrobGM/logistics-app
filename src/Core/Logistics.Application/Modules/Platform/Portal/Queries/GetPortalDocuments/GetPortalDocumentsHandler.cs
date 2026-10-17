using Logistics.Application.Abstractions;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Mediator;
using Logistics.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace Logistics.Application.Modules.Platform.Portal.Queries;

internal sealed class GetPortalDocumentsHandler(ITenantUnitOfWork tenantUow)
    : IRequestHandler<GetPortalDocumentsQuery, Result<PortalDocumentDto[]>>
{
    public async Task<Result<PortalDocumentDto[]>> Handle(
        GetPortalDocumentsQuery req,
        CancellationToken ct)
    {
        var documents = await tenantUow.Repository<LoadDocument>().Query()
            .Where(d => d.Load.CustomerId == req.CustomerId)
            .ToPortalDocuments()
            .ToArrayAsync(ct);

        return Result<PortalDocumentDto[]>.Ok(documents);
    }
}
