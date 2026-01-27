using Logistics.Application.Abstractions;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Mappings;
using Logistics.Shared.Models;

namespace Logistics.Application.Queries;

internal sealed class GetLoadExceptionsHandler(ITenantUnitOfWork tenantUow)
    : IAppRequestHandler<GetLoadExceptionsQuery, Result<IEnumerable<LoadExceptionDto>>>
{
    public async Task<Result<IEnumerable<LoadExceptionDto>>> Handle(GetLoadExceptionsQuery req, CancellationToken ct)
    {
        var exceptions = await tenantUow.Repository<LoadException>()
            .GetListAsync(e => e.LoadId == req.LoadId, ct);

        var sortedExceptions = exceptions
            .OrderByDescending(e => e.OccurredAt)
            .Select(e => e.ToDto())
            .ToList();

        return Result<IEnumerable<LoadExceptionDto>>.Ok(sortedExceptions);
    }
}
