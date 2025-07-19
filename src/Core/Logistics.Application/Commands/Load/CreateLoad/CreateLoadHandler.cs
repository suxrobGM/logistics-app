using Logistics.Application.Extensions;
using Logistics.Application.Services;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Shared.Models;

namespace Logistics.Application.Commands;

internal sealed class CreateLoadHandler : RequestHandler<CreateLoadCommand, Result>
{
    private readonly ITenantUnityOfWork _tenantUow;
    private readonly IPushNotificationService _pushNotificationService;

    public CreateLoadHandler(
        ITenantUnityOfWork tenantUow,
        IPushNotificationService pushNotificationService)
    {
        _tenantUow = tenantUow;
        _pushNotificationService = pushNotificationService;
    }

    protected override async Task<Result> HandleValidated(
        CreateLoadCommand req, CancellationToken cancellationToken)
    {
        var dispatcher = await _tenantUow.Repository<Employee>().GetByIdAsync(req.AssignedDispatcherId);

        if (dispatcher is null)
        {
            return Result.Fail("Could not find the specified dispatcher");
        }

        var truck = await _tenantUow.Repository<Truck>().GetByIdAsync(req.AssignedTruckId);

        if (truck is null)
        {
            return Result.Fail($"Could not find the truck with ID '{req.AssignedTruckId}'");
        }
        
        var customer = await _tenantUow.Repository<Customer>().GetByIdAsync(req.CustomerId);

        if (customer is null)
        {
            return Result.Fail($"Could not find the customer with ID '{req.CustomerId}'");
        }
        
        var load = Load.Create(
            req.Name,
            req.Type,
            req.DeliveryCost,
            req.OriginAddress!,
            req.OriginAddressLat,
            req.OriginAddressLong,
            req.DestinationAddress!,
            req.DestinationAddressLat,
            req.DestinationAddressLong,
            customer,
            truck, 
            dispatcher);
        
        load.Name = req.Name;
        load.Distance = req.Distance;

        await _tenantUow.Repository<Load>().AddAsync(load);
        var changes = await _tenantUow.SaveChangesAsync();

        if (changes > 0)
        {
            await _pushNotificationService.SendNewLoadNotificationAsync(load, truck);
        }
        return Result.Succeed();
    }
}
