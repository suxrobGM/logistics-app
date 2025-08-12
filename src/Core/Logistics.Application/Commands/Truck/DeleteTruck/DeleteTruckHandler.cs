using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Shared.Models;

namespace Logistics.Application.Commands;

internal sealed class DeleteTruckHandler : RequestHandler<DeleteTruckCommand, Result>
{
    private readonly ITenantUnityOfWork _tenantUow;

    public DeleteTruckHandler(ITenantUnityOfWork tenantUow)
    {
        _tenantUow = tenantUow;
    }

    protected override async Task<Result> HandleValidated(
        DeleteTruckCommand req, CancellationToken cancellationToken)
    {
        var truck = await _tenantUow.Repository<Truck>().GetByIdAsync(req.Id);
        _tenantUow.Repository<Truck>().Delete(truck);
        await _tenantUow.SaveChangesAsync();
        return Result.Succeed();
    }
}
