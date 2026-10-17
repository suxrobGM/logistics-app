using Logistics.Application.Abstractions;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Mappings;
using Logistics.Mediator;
using Logistics.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace Logistics.Application.Modules.Platform.Portal.Queries;

internal sealed class GetPortalDocumentsHandler(ITenantUnitOfWork tenantUow)
    : IRequestHandler<GetPortalDocumentsQuery, Result<IEnumerable<DocumentDto>>>
{
    public async Task<Result<IEnumerable<DocumentDto>>> Handle(
        GetPortalDocumentsQuery req,
        CancellationToken ct)
    {
        var documents = await tenantUow.Repository<LoadDocument>().Query()
            .Where(d => d.Load.CustomerId == req.CustomerId)
            .OrderByDescending(d => d.CreatedAt)
            .ToListAsync(ct);

        var dtos = documents.Select(d => d.ToDto());
        return Result<IEnumerable<DocumentDto>>.Ok(dtos);
    }
}
