using Logistics.Application.Abstractions;
using Logistics.Application.Modules.Financial.Factoring.Services;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Mediator;
using Logistics.Shared.Models;

namespace Logistics.Application.Modules.Financial.Factoring.Queries;

internal sealed class GetFactoringBatchByIdHandler(ITenantUnitOfWork tenantUow, IFactoringBatchDtoBuilder batchDtoBuilder)
    : IRequestHandler<GetFactoringBatchByIdQuery, Result<FactoringBatchDto>>
{
    public async Task<Result<FactoringBatchDto>> Handle(GetFactoringBatchByIdQuery req, CancellationToken ct)
    {
        var batch = await tenantUow.Repository<FactoringBatch>().GetByIdAsync(req.Id, ct);
        return batch is null
            ? Result<FactoringBatchDto>.Fail("Schedule of accounts not found.")
            : Result<FactoringBatchDto>.Ok(await batchDtoBuilder.BuildAsync(batch, ct));
    }
}
