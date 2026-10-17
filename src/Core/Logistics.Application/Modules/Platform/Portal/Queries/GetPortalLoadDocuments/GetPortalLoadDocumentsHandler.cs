using Logistics.Application.Abstractions;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Mediator;
using Logistics.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace Logistics.Application.Modules.Platform.Portal.Queries;

internal sealed class GetPortalLoadDocumentsHandler(ITenantUnitOfWork tenantUow)
    : IRequestHandler<GetPortalLoadDocumentsQuery, Result<PortalDocumentDto[]>>
{
    public async Task<Result<PortalDocumentDto[]>> Handle(
        GetPortalLoadDocumentsQuery req,
        CancellationToken ct)
    {
        // Verify the load belongs to the customer
        var loadExists = await tenantUow.Repository<Load>().Query()
            .AnyAsync(l => l.Id == req.LoadId && l.CustomerId == req.CustomerId, ct);

        if (!loadExists)
        {
            return Result<PortalDocumentDto[]>.Fail("Load not found or access denied.");
        }

        var documents = await tenantUow.Repository<LoadDocument>().Query()
            .Where(d => d.LoadId == req.LoadId)
            .ToPortalDocuments()
            .ToArrayAsync(ct);

        return Result<PortalDocumentDto[]>.Ok(documents);
    }
}
