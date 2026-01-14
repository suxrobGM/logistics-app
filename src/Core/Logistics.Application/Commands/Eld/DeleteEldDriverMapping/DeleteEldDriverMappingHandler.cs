using Logistics.Application.Abstractions;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Shared.Models;

namespace Logistics.Application.Commands;

internal sealed class DeleteEldDriverMappingHandler(ITenantUnitOfWork tenantUow)
    : IAppRequestHandler<DeleteEldDriverMappingCommand, Result>
{
    public async Task<Result> Handle(DeleteEldDriverMappingCommand req, CancellationToken ct)
    {
        var mapping = await tenantUow.Repository<EldDriverMapping>()
            .GetByIdAsync(req.MappingId, ct);

        if (mapping is null)
        {
            return Result.Fail("Driver mapping not found");
        }

        // Also delete associated HOS status if exists
        var hosStatus = await tenantUow.Repository<DriverHosStatus>()
            .GetAsync(h => h.EmployeeId == mapping.EmployeeId && h.ProviderType == mapping.ProviderType, ct);

        if (hosStatus is not null)
        {
            tenantUow.Repository<DriverHosStatus>().Delete(hosStatus);
        }

        tenantUow.Repository<EldDriverMapping>().Delete(mapping);
        await tenantUow.SaveChangesAsync(ct);

        return Result.Ok();
    }
}
