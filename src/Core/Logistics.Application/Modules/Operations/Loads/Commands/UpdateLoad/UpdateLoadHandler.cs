using Logistics.Application.Abstractions;
using Logistics.Application.Modules.Operations.Common.Services;
using Logistics.Application.Modules.Operations.Loads.Services;
using Logistics.Application.Utilities;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Mediator;
using Logistics.Shared.Models;

namespace Logistics.Application.Modules.Operations.Loads.Commands;

internal sealed class UpdateLoadHandler(
    ITenantUnitOfWork tenantUow,
    IVehicleTransportGuard vehicleTransportGuard,
    ILoadChargeService loadChargeService)
    : IRequestHandler<UpdateLoadCommand, Result>
{
    public async Task<Result> Handle(UpdateLoadCommand req, CancellationToken ct)
    {
        var load = await tenantUow.Repository<Load>().GetByIdAsync(req.Id, ct);

        if (load is null)
        {
            return Result.Fail("Could not find the specified load");
        }

        // Only a change is checked, so existing vehicle loads stay editable after the feature is turned off.
        var typeCheck = await vehicleTransportGuard.CheckLoadTypeAsync(req.Type == load.Type ? null : req.Type);
        if (!typeCheck.IsSuccess)
        {
            return typeCheck;
        }

        var chargesBefore = ChargeInputs(load);

        try
        {
            var truckChanged = await AssignTruckIfUpdated(req, load);

            await AssignDispatcherIfUpdated(req, load);
            await UpdateCustomerIfUpdated(req, load);

            load.Name = PropertyUpdater.UpdateIfChanged(req.Name, load.Name);
            load.OriginAddress = PropertyUpdater.UpdateIfChanged(req.OriginAddress, load.OriginAddress);
            load.OriginLocation = PropertyUpdater.UpdateIfChanged(req.OriginLocation, load.OriginLocation);
            load.DestinationAddress = PropertyUpdater.UpdateIfChanged(req.DestinationAddress, load.DestinationAddress);
            load.DestinationLocation = PropertyUpdater.UpdateIfChanged(req.DestinationLocation, load.DestinationLocation);

            if (req.DeliveryCost is not null && req.DeliveryCost != load.DeliveryCost.Amount)
            {
                load.DeliveryCost = new() { Amount = req.DeliveryCost.Value, Currency = load.DeliveryCost.Currency };
            }

            load.Distance = PropertyUpdater.UpdateIfChanged(req.Distance, load.Distance);
            load.Type = PropertyUpdater.UpdateIfChanged(req.Type, load.Type);
            load.Source = PropertyUpdater.UpdateIfChanged(req.Source, load.Source);
            load.Notes = PropertyUpdater.UpdateIfChanged(req.Notes, load.Notes);

            // Nullable<T>-to-Nullable<T> assignments - PropertyUpdater's T : struct constraint can't take Nullable<T> as T.
            if (req.RequestedPickupDate.HasValue) load.RequestedPickupDate = req.RequestedPickupDate;
            if (req.RequestedDeliveryDate.HasValue) load.RequestedDeliveryDate = req.RequestedDeliveryDate;
            if (req.ContainerId.HasValue) load.ContainerId = req.ContainerId;
            if (req.OriginTerminalId.HasValue) load.OriginTerminalId = req.OriginTerminalId;
            if (req.DestinationTerminalId.HasValue) load.DestinationTerminalId = req.DestinationTerminalId;
            if (req.IsHazmat.HasValue) load.IsHazmat = req.IsHazmat.Value;
            if (req.HazmatClass.HasValue) load.HazmatClass = req.HazmatClass;
            if (req.UnNumber is not null) load.UnNumber = req.UnNumber;

            // Also runs without new vehicles, so a type change away from Vehicle cannot strand the old ones.
            load.SetVehicles(req.Vehicles?.ToLoadVehicles() ?? load.Vehicles);

            if (req.Status.HasValue)
            {
                load.UpdateStatus(req.Status.Value, force: false);
                load.RefreshTripStatus();
            }

            // Raise LoadUpdatedEvent for existing truck (if not changing truck)
            // Truck change events are handled by AssignToTruck method
            if (!truckChanged)
            {
                load.MarkUpdated();
            }

            await ApplyShipmentChangesAsync(req, load, ct);

            // A rebuild bumps the invoice's UpdatedAt, which re-pushes it to the accounting system.
            if (!ChargeInputs(load).SequenceEqual(chargesBefore))
            {
                await loadChargeService.SyncInvoiceAsync(load, ct);
            }

            // SaveChanges dispatches domain events:
            // - LoadAssignedToTruckEvent (if truck assigned)
            // - LoadRemovedFromTruckEvent (if truck changed, for old truck)
            // - LoadUpdatedEvent (if truck not changed but details updated)
            await tenantUow.SaveChangesAsync(ct);

            return Result.Ok();
        }
        catch (InvalidOperationException ex)
        {
            return Result.Fail(ex.Message);
        }
    }

    /// <summary>
    ///     Every load value <see cref="LoadInvoice.ReplaceLoadCharges" /> reads to build the invoice lines.
    /// </summary>
    private static List<object> ChargeInputs(Load load) =>
    [
        load.Type,
        load.DeliveryCost.Amount,
        load.DeliveryCost.Currency,
        load.FuelSurcharge,
        .. load.Accessorials.Select(a => (a.Id, a.Name, a.LineItemType, a.Rate, a.Quantity))
    ];

    private async Task ApplyShipmentChangesAsync(UpdateLoadCommand req, Load load, CancellationToken ct)
    {
        if (req.FreightClass.HasValue) load.FreightClass = req.FreightClass;
        if (req.NmfcCode is not null) load.NmfcCode = req.NmfcCode;
        if (req.PalletCount.HasValue) load.PalletCount = req.PalletCount;
        if (req.PieceCount.HasValue) load.PieceCount = req.PieceCount;
        if (req.Weight.HasValue) load.Weight = req.Weight;
        if (req.Dimensions is not null) load.Dimensions = req.Dimensions;
        if (req.IsStackable.HasValue) load.IsStackable = req.IsStackable.Value;

        if (req.FuelSurcharge.HasValue) load.FuelSurcharge = req.FuelSurcharge.Value;

        if (req.Accessorials is not null)
        {
            await loadChargeService.ReplaceAccessorialsAsync(load, req.Accessorials, ct);
        }
    }

    private async Task UpdateCustomerIfUpdated(UpdateLoadCommand req, Load loadEntity)
    {
        if (req.CustomerId is null)
        {
            return;
        }

        var customer = await tenantUow.Repository<Customer>().GetByIdAsync(req.CustomerId.Value)
            ?? throw new InvalidOperationException($"Could not find a customer with ID '{req.CustomerId}'");

        if (loadEntity.CustomerId != customer.Id)
        {
            loadEntity.Customer = customer;
        }
    }

    private async Task<bool> AssignTruckIfUpdated(UpdateLoadCommand req, Load loadEntity)
    {
        if (req.AssignedTruckId is null)
        {
            return false;
        }

        var truck = await tenantUow.Repository<Truck>().GetByIdAsync(req.AssignedTruckId.Value);
        if (truck is null)
        {
            throw new InvalidOperationException($"Could not find a truck with ID '{req.AssignedTruckId}'");
        }

        if (loadEntity.AssignedTruckId == truck.Id)
        {
            return false;
        }

        // Use domain method which raises LoadAssignedToTruckEvent for notifications
        loadEntity.AssignToTruck(truck);
        return true;
    }

    private async Task AssignDispatcherIfUpdated(UpdateLoadCommand req, Load loadEntity)
    {
        if (req.AssignedDispatcherId is null)
        {
            return;
        }

        var dispatcher = await tenantUow.Repository<Employee>().GetByIdAsync(req.AssignedDispatcherId.Value);
        if (dispatcher is null)
        {
            throw new InvalidOperationException($"Could not find a dispatcher with ID '{req.AssignedDispatcherId}'");
        }

        if (loadEntity.AssignedDispatcherId != dispatcher.Id)
        {
            loadEntity.AssignedDispatcher = dispatcher;
        }
    }
}
