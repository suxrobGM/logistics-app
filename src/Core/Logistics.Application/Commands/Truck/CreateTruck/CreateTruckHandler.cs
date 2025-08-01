﻿using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Shared.Models;

namespace Logistics.Application.Commands;

internal sealed class CreateTruckHandler : RequestHandler<CreateTruckCommand, Result>
{
    private readonly ITenantUnityOfWork _tenantUow;

    public CreateTruckHandler(ITenantUnityOfWork tenantUow)
    {
        _tenantUow = tenantUow;
    }

    protected override async Task<Result> HandleValidated(
        CreateTruckCommand req, CancellationToken cancellationToken)
    {
        var truckWithThisNumber = await _tenantUow.Repository<Truck>().GetAsync(i => i.Number == req.TruckNumber);

        if (truckWithThisNumber is not null)
        {
            return Result.Fail($"Already exists truck with number {req.TruckNumber}");
        }

        var driver = await _tenantUow.Repository<Employee>().GetByIdAsync(req.MainDriverId);

        if (driver is null)
        {
            return Result.Fail($"Could not find driver with ID {req.MainDriverId}");
        }

        var alreadyAssociatedTruck = await _tenantUow.Repository<Truck>().GetAsync(i => i.MainDriverId == driver.Id);

        if (alreadyAssociatedTruck is not null)
        {
            return Result.Fail($"Driver '{driver.GetFullName()}' is already associated with the truck number '{req.TruckNumber}'");
        }
        
        var truckEntity = Truck.Create(req.TruckNumber, req.TruckType, driver);
        await _tenantUow.Repository<Truck>().AddAsync(truckEntity);
        await _tenantUow.SaveChangesAsync();
        return Result.Succeed();
    }
}
