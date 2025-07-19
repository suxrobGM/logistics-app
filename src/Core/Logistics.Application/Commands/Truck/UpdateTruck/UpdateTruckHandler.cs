using Logistics.Application.Specifications;
using Logistics.Application.Utilities;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Shared.Models;

namespace Logistics.Application.Commands;

internal sealed class UpdateTruckHandler : RequestHandler<UpdateTruckCommand, Result>
{
    private readonly ITenantUnityOfWork _tenantUow;

    public UpdateTruckHandler(ITenantUnityOfWork tenantUow)
    {
        _tenantUow = tenantUow;
    }

    protected override async Task<Result> HandleValidated(
        UpdateTruckCommand req, CancellationToken cancellationToken)
    {
        var truckRepository = _tenantUow.Repository<Truck>();
        var truckEntity = await truckRepository.GetByIdAsync(req.Id);

        if (truckEntity is null)
        {
            return Result.Fail("Could not find the specified truck");
        }
        
        var truckWithThisNumber = await truckRepository.GetAsync(i => i.Number == req.TruckNumber && 
                                                                             i.Id != truckEntity.Id);
        if (truckWithThisNumber is not null)
        {
            return Result.Fail("Already exists truck with this number");
        }
        
        if (req.DriverIds != null)
        {
            var drivers = _tenantUow.Repository<Employee>()
                .ApplySpecification(new GetEmployeesById(req.DriverIds))
                .ToList();
            
            if (drivers.Count != 0)
                truckEntity.Drivers = drivers;
        }

        truckEntity.Number = PropertyUpdater.UpdateIfChanged(req.TruckNumber, truckEntity.Number);
        truckEntity.Type = PropertyUpdater.UpdateIfChanged(req.TruckType, truckEntity.Type);
        truckEntity.Status = PropertyUpdater.UpdateIfChanged(req.TruckStatus, truckEntity.Status);
        
        truckRepository.Update(truckEntity);
        await _tenantUow.SaveChangesAsync();
        return Result.Succeed();
    }
}
