using Logistics.Application.Abstractions;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Shared.Models;

namespace Logistics.Application.Commands;

internal sealed class DeleteTruckHandler : IAppRequestHandler<DeleteTruckCommand, Result>
{
    private readonly ITenantUnitOfWork _tenantUow;

    public DeleteTruckHandler(ITenantUnitOfWork tenantUow)
    {
        _tenantUow = tenantUow;
    }

    public async Task<Result> Handle(
        DeleteTruckCommand req, CancellationToken ct)
    {
        var truck = await _tenantUow.Repository<Truck>().GetByIdAsync(req.Id);
        _tenantUow.Repository<Truck>().Delete(truck);
        await _tenantUow.SaveChangesAsync();
        return Result.Succeed();
    }
}
