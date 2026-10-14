using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Mediator;
using Logistics.Shared.Models;

namespace Logistics.Application.Modules.Compliance.Eld.Commands;

internal sealed class DeleteEldVehicleMappingHandler(ITenantUnitOfWork tenantUow)
    : IRequestHandler<DeleteEldVehicleMappingCommand, Result>
{
    public async Task<Result> Handle(DeleteEldVehicleMappingCommand req, CancellationToken ct)
    {
        var mapping = await tenantUow.Repository<EldVehicleMapping>().GetByIdAsync(req.MappingId, ct);
        if (mapping is null)
        {
            return Result.Fail("Vehicle mapping not found");
        }

        tenantUow.Repository<EldVehicleMapping>().Delete(mapping);
        await tenantUow.SaveChangesAsync(ct);
        return Result.Ok();
    }
}
