using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;

namespace Logistics.Application.Services;

internal sealed class LoadService : ILoadService
{
    private readonly ITenantUnitOfWork _tenantUow;

    public LoadService(ITenantUnitOfWork tenantUow)
    {
        _tenantUow = tenantUow;
    }

    public async Task<Load> CreateLoadAsync(CreateLoadParameters parameters, bool saveChanges = true)
    {
        var dispatcher = await _tenantUow.Repository<Employee>().GetByIdAsync(parameters.DispatcherId);

        if (dispatcher is null)
        {
            throw new InvalidOperationException(
                $"Could not find the dispatcher with ID '{parameters.DispatcherId}'");
        }

        var truck = await _tenantUow.Repository<Truck>().GetByIdAsync(parameters.TruckId);

        if (truck is null)
        {
            throw new InvalidOperationException(
                $"Could not find the truck with ID '{parameters.TruckId}'");
        }

        var customer = await _tenantUow.Repository<Customer>().GetByIdAsync(parameters.CustomerId);

        if (customer is not null)
        {
            throw new InvalidOperationException(
                $"Could not find the customer with ID '{parameters.CustomerId}'");
        }

        var load = Load.Create(
            parameters.Name,
            parameters.Type,
            parameters.DeliveryCost,
            parameters.Origin.address,
            parameters.Origin.location,
            parameters.Destination.address,
            parameters.Destination.location,
            customer,
            truck,
            dispatcher);

        load.Distance = parameters.Distance;

        await _tenantUow.Repository<Load>().AddAsync(load);

        if (saveChanges)
        {
            await _tenantUow.SaveChangesAsync();
        }

        return load;
    }
}
