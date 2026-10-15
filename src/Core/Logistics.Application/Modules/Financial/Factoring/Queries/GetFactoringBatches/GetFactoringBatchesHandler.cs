using Logistics.Application.Abstractions;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Mappings;
using Logistics.Mediator;
using Logistics.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace Logistics.Application.Modules.Financial.Factoring.Queries;

internal sealed class GetFactoringBatchesHandler(ITenantUnitOfWork tenantUow)
    : IRequestHandler<GetFactoringBatchesQuery, PagedResult<FactoringBatchDto>>
{
    public async Task<PagedResult<FactoringBatchDto>> Handle(GetFactoringBatchesQuery req, CancellationToken ct)
    {
        var query = tenantUow.Repository<FactoringBatch>().Query();
        if (req.Status is { } status)
        {
            query = query.Where(b => b.Status == status);
        }

        if (req.FactoringCompanyId is { } companyId)
        {
            query = query.Where(b => b.FactoringCompanyId == companyId);
        }

        var totalItems = await query.CountAsync(ct);
        var batches = await query
            .OrderBy(string.IsNullOrEmpty(req.OrderBy) ? "-SubmittedAt" : req.OrderBy)
            .ApplyPaging(req.Page, req.PageSize)
            .ToListAsync(ct);

        var batchIds = batches.Select(b => b.Id).ToList();
        var items = (await tenantUow.Repository<FactoringBatchItem>().GetListAsync(i => batchIds.Contains(i.BatchId), ct))
            .ToLookup(i => i.BatchId);
        var companyNames = await tenantUow.Repository<FactoringCompany>().Query()
            .ToDictionaryAsync(f => f.Id, f => f.Name, ct);

        var dtos = batches
            .Select(b => b.ToDto(companyNames.GetValueOrDefault(b.FactoringCompanyId), items[b.Id].ToList()))
            .ToArray();
        return PagedResult<FactoringBatchDto>.Ok(dtos, totalItems, req.PageSize);
    }
}
