using Logistics.Application.Tenant.Extensions;
using Logistics.Application.Tenant.Services;

namespace Logistics.Application.Tenant.Commands;

internal sealed class UpdateLoadHandler : RequestHandler<UpdateLoadCommand, ResponseResult>
{
    private readonly ITenantRepository _tenantRepository;
    private readonly IPushNotification _pushNotification;

    public UpdateLoadHandler(
        ITenantRepository tenantRepository,
        IPushNotification pushNotification)
    {
        _tenantRepository = tenantRepository;
        _pushNotification = pushNotification;
    }

    protected override async Task<ResponseResult> HandleValidated(
            UpdateLoadCommand req, CancellationToken cancellationToken)
        {
            var loadEntity = await _tenantRepository.GetAsync<Load>(req.Id);
            if (loadEntity == null)
                return ResponseResult.CreateError("Could not find the specified load");

            try
            {
                var oldTruck = loadEntity.AssignedTruck;
                var newTruck = await AssignTruckIfUpdated(req, loadEntity);

                await AssignDispatcherIfUpdated(req, loadEntity);
                var updatedDetails = UpdateLoadDetails(req, loadEntity);

                _tenantRepository.Update(loadEntity);
                var changes = await _tenantRepository.UnitOfWork.CommitAsync();

                if (changes > 0)
                {
                    await NotifyTrucksAboutUpdates(updatedDetails, oldTruck, newTruck, loadEntity);
                }
                
                return ResponseResult.CreateSuccess();
            }
            catch (InvalidOperationException ex)
            {
                return ResponseResult.CreateError(ex.Message);
            }
        }

        private async Task<Truck?> AssignTruckIfUpdated(UpdateLoadCommand req, Load loadEntity)
        {
            if (req.AssignedTruckId is null) 
                return null;

            var truck = await _tenantRepository.GetAsync<Truck>(req.AssignedTruckId);
            if (truck is null)
            {
                throw new InvalidOperationException($"Could not find a truck with ID '{req.AssignedTruckId}'");
            }

            if (loadEntity.AssignedTruckId != truck.Id)
            {
                loadEntity.AssignedTruck = truck;
                return truck;
            }

            return null;
        }

        private async Task AssignDispatcherIfUpdated(UpdateLoadCommand req, Load loadEntity)
        {
            if (req.AssignedDispatcherId is null) 
                return;

            var dispatcher = await _tenantRepository.GetAsync<Employee>(req.AssignedDispatcherId);
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

            if (req.Name != null && req.Name != load.Name)
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
            
            if (req.CanConfirmPickUp.HasValue && req.CanConfirmPickUp != load.CanConfirmPickUp)
            {
                load.CanConfirmPickUp = req.CanConfirmPickUp.Value;
                updated = true;
            }
            
            if (req.CanConfirmDelivery.HasValue && req.CanConfirmDelivery != load.CanConfirmDelivery)
            {
                load.CanConfirmDelivery = req.CanConfirmDelivery.Value;

                if (req.CanConfirmDelivery.Value)
                {
                    load.CanConfirmPickUp = true;
                }
                
                updated = true;
            }

            return updated;
        }

        private async Task NotifyTrucksAboutUpdates(bool detailsUpdated, Truck? oldTruck, Truck? newTruck, Load loadEntity)
        {
            if (detailsUpdated && oldTruck != null)
            {
                // send updates to the old truck
                await _pushNotification.SendUpdatedLoadNotificationAsync(loadEntity, oldTruck);
            }
            if (newTruck != null && oldTruck != null && oldTruck.Id != newTruck.Id)
            {
                // The truck was switched
                await _pushNotification.SendNewLoadNotificationAsync(loadEntity, newTruck);
                await _pushNotification.SendRemovedLoadNotificationAsync(loadEntity, oldTruck);
            }
        }
}
