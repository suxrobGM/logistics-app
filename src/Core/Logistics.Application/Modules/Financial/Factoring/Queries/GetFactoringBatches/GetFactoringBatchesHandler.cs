using Logistics.Application.Abstractions;
using Logistics.Application.Common.Extensions;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
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

        return await query.ToPagedResultAsync(req, MapPageAsync, ct, defaultOrderBy: "-SubmittedAt");
    }

    private async Task<IEnumerable<FactoringBatchDto>> MapPageAsync(
        IReadOnlyList<FactoringBatch> batches,
        CancellationToken ct)
    {
        var batchIds = batches.Select(b => b.Id).ToList();
        var items = (await tenantUow.Repository<FactoringBatchItem>().GetListAsync(i => batchIds.Contains(i.BatchId), ct))
            .ToLookup(i => i.BatchId);
        var companyNames = await tenantUow.Repository<FactoringCompany>().Query()
            .Select(f => new { f.Id, f.Name })
            .ToDictionaryAsync(f => f.Id, f => f.Name, ct);

        return batches.Select(b => b.ToDto(companyNames.GetValueOrDefault(b.FactoringCompanyId), items[b.Id].ToList()));
    }
}
