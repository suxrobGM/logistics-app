using Logistics.Application.Extensions;
using Logistics.Application.Services;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Shared.Models;

namespace Logistics.Application.Commands;

internal sealed class UpdateLoadHandler : RequestHandler<UpdateLoadCommand, Result>
{
    private readonly ITenantUnityOfWork _tenantUow;
    private readonly IPushNotificationService _pushNotificationService;

    public UpdateLoadHandler(
        ITenantUnityOfWork tenantUow,
        IPushNotificationService pushNotificationService)
    {
        _tenantUow = tenantUow;
        _pushNotificationService = pushNotificationService;
    }

    protected override async Task<Result> HandleValidated(
            UpdateLoadCommand req, CancellationToken cancellationToken)
        {
            var loadEntity = await _tenantUow.Repository<Load>().GetByIdAsync(req.Id);
            
            if (loadEntity is null)
            {
                return Result.Fail("Could not find the specified load");
            }

            try
            {
                var oldTruck = loadEntity.AssignedTruck;
                var newTruck = await AssignTruckIfUpdated(req, loadEntity);

                await AssignDispatcherIfUpdated(req, loadEntity);
                await UpdateCustomerIfUpdated(req, loadEntity);
                var updatedDetails = UpdateLoadDetails(req, loadEntity);

                _tenantUow.Repository<Load>().Update(loadEntity);
                var changes = await _tenantUow.SaveChangesAsync();

                if (changes > 0)
                {
                    await NotifyTrucksAboutUpdates(updatedDetails, oldTruck, newTruck, loadEntity);
                }
                
                return Result.Succeed();
            }
            catch (InvalidOperationException ex)
            {
                return Result.Fail(ex.Message);
            }
        }
    
        private async Task UpdateCustomerIfUpdated(UpdateLoadCommand req, Load loadEntity)
        {
            if (req.CustomerId is null)
            {
                return;
            }

            var customer = await _tenantUow.Repository<Customer>().GetByIdAsync(req.CustomerId.Value);
            if (customer is null)
            {
                throw new InvalidOperationException($"Could not find a customer with ID '{req.CustomerId}'");
            }

            if (loadEntity.CustomerId != customer.Id)
            {
                loadEntity.Customer = customer;
            }
        }

        private async Task<Truck?> AssignTruckIfUpdated(UpdateLoadCommand req, Load loadEntity)
        {
            if (req.AssignedTruckId is null)
            {
                return null;
            }

            var truck = await _tenantUow.Repository<Truck>().GetByIdAsync(req.AssignedTruckId.Value);
            if (truck is null)
            {
                throw new InvalidOperationException($"Could not find a truck with ID '{req.AssignedTruckId}'");
            }

            if (loadEntity.AssignedTruckId == truck.Id)
            {
                return null;
            }
            
            loadEntity.AssignedTruck = truck;
            return truck;

        }

        private async Task AssignDispatcherIfUpdated(UpdateLoadCommand req, Load loadEntity)
        {
            if (req.AssignedDispatcherId is null)
            {
                return;
            }

            var dispatcher = await _tenantUow.Repository<Employee>().GetByIdAsync(req.AssignedDispatcherId.Value);
            if (dispatcher is null)
            {
                throw new InvalidOperationException($"Could not find a dispatcher with ID '{req.AssignedDispatcherId}'");
            }

            if (loadEntity.AssignedDispatcherId != dispatcher.Id)
            {
                loadEntity.AssignedDispatcher = dispatcher;
            }
        }

        private bool UpdateLoadDetails(UpdateLoadCommand req, Load load)
        {
            var updated = false;

            if (req.Name is not null && req.Name != load.Name)
            {
                load.Name = req.Name;
                updated = true;
            }

            if (req.OriginAddress != null && req.OriginAddress != load.OriginAddress)
            {
                load.OriginAddress = req.OriginAddress;
                load.OriginAddressLat = req.OriginAddressLat;
                load.OriginAddressLong = req.OriginAddressLong;
                updated = true;
            }

            if (req.DestinationAddress != null && req.DestinationAddress != load.DestinationAddress)
            {
                load.DestinationAddress = req.DestinationAddress;
                load.DestinationAddressLat = req.DestinationAddressLat;
                load.DestinationAddressLong = req.DestinationAddressLong;
                updated = true;
            }
            
            if (req.DeliveryCost.HasValue && req.DeliveryCost != load.DeliveryCost)
            {
                load.DeliveryCost = req.DeliveryCost.Value;
                updated = true;
            }
            
            if (req.Distance.HasValue && req.Distance != load.Distance)
            {
                load.Distance = req.Distance.Value;
                updated = true;
            }
            
            if (req.Status.HasValue && req.Status != load.GetStatus())
            {
                load.SetStatus(req.Status.Value);
                updated = true;
            }

            return updated;
        }

        private async Task NotifyTrucksAboutUpdates(bool detailsUpdated, Truck? oldTruck, Truck? newTruck, Load loadEntity)
        {
            if (detailsUpdated && oldTruck != null)
            {
                // send updates to the old truck
                await _pushNotificationService.SendUpdatedLoadNotificationAsync(loadEntity, oldTruck);
            }
            if (newTruck != null && oldTruck != null && oldTruck.Id != newTruck.Id)
            {
                // The truck was switched
                await _pushNotificationService.SendNewLoadNotificationAsync(loadEntity, newTruck);
                await _pushNotificationService.SendRemovedLoadNotificationAsync(loadEntity, oldTruck);
            }
        }
}
