using Logistics.Application.Specifications;
using Logistics.Domain.Entities;
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
        
        var drivers = _tenantUow.Repository<Employee>()
            .ApplySpecification(new GetEmployeesById(req.DriversIds!))
            .ToList();

        if (drivers.Count == 0)
        {
            return Result.Fail("Could not find any drivers with specified IDs");
        }

        var alreadyAssociatedDriver = drivers.FirstOrDefault(i => i.Truck != null && i.Truck.Number == req.TruckNumber);

        if (alreadyAssociatedDriver is not null)
        {
            return Result.Fail($"Driver '{alreadyAssociatedDriver.GetFullName()}' is already associated with the truck number '{req.TruckNumber}'");
        }
        
        var truckEntity = Truck.Create(req.TruckNumber!, drivers);
        await _tenantUow.Repository<Truck>().AddAsync(truckEntity);
        await _tenantUow.SaveChangesAsync();
        return Result.Succeed();
    }
}
