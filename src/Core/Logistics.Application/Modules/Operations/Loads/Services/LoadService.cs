using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Shared.Models;

namespace Logistics.Application.Modules.Operations.Loads.Services;

internal sealed class LoadService(ITenantUnitOfWork tenantUow, IVehicleTransportFeatureChecker vehicleTransportFeatureChecker)
    : ILoadService
{
    public async Task<Result<Load>> CreateLoadAsync(NewLoad newLoad, bool saveChanges = true,
        CancellationToken ct = default)
    {
        var created = await CreateLoadsAsync([newLoad], saveChanges, ct);
        return created.IsSuccess
            ? Result<Load>.Ok(created.Value![0])
            : Result<Load>.Fail(created);
    }

    public async Task<Result<IReadOnlyList<Load>>> CreateLoadsAsync(IEnumerable<NewLoad> newLoads,
        bool saveChanges = true, CancellationToken ct = default)
    {
        var newLoadList = newLoads as IList<NewLoad> ?? newLoads.ToList();
        if (newLoadList.Count == 0)
        {
            return Result<IReadOnlyList<Load>>.Ok([]);
        }

        var typeCheck = await vehicleTransportFeatureChecker.CheckLoadTypesAsync(newLoadList.Select(p => p.Details.Type));
        if (!typeCheck.IsSuccess)
        {
            return Result<IReadOnlyList<Load>>.Fail(typeCheck);
        }

        // 1) Collect distinct foreign keys (filter out null truck IDs)
        var dispatcherIds = newLoadList.Select(p => p.Details.AssignedDispatcherId).Distinct().ToList();
        var truckIds = newLoadList.Where(p => p.TruckId.HasValue).Select(p => p.TruckId!.Value).Distinct().ToList();
        var customerIds = newLoadList.Select(p => p.Details.CustomerId).Distinct().ToList();
        var tripIds = newLoadList.Select(p => p.TripId).Distinct().ToList();

        // 2) Batch-load referenced rows (at most 3 queries total)
        var dispatcherRepo = tenantUow.Repository<Employee>();
        var truckRepo = tenantUow.Repository<Truck>();
        var customerRepo = tenantUow.Repository<Customer>();
        var tripRepo = tenantUow.Repository<Trip>();

        // Note: EF Core DbContext does not allow parallel queries on the same context;
        // keep these sequential to avoid InvalidOperationException.
        var dispatchers = await dispatcherRepo.GetListAsync(e => dispatcherIds.Contains(e.Id), ct);
        var trucks = await truckRepo.GetListAsync(t => truckIds.Contains(t.Id), ct);
        var customers = await customerRepo.GetListAsync(c => customerIds.Contains(c.Id), ct);
        var trips = await tripRepo.GetListAsync(t => tripIds.Contains(t.Id), ct);

        var dispatcherById = dispatchers.ToDictionary(e => e.Id);
        var truckById = trucks.ToDictionary(t => t.Id);
        var customerById = customers.ToDictionary(c => c.Id);
        var tripById = trips.ToDictionary(t => t.Id);

        // 3) Validate missing references (fail fast with precise info)
        var missingDispatchers = dispatcherIds.Where(id => !dispatcherById.ContainsKey(id)).ToList();
        var missingTrucks = truckIds.Where(id => !truckById.ContainsKey(id)).ToList();
        var missingCustomers = customerIds.Where(id => !customerById.ContainsKey(id)).ToList();

        if (missingDispatchers.Count > 0 || missingTrucks.Count > 0 || missingCustomers.Count > 0)
        {
            // Aggregate into a single, developer-friendly error message
            var problems = new List<string>(3);
            if (missingDispatchers.Count > 0)
            {
                problems.Add($"Dispatchers missing: {string.Join(", ", missingDispatchers)}");
            }

            if (missingTrucks.Count > 0)
            {
                problems.Add($"Trucks missing: {string.Join(", ", missingTrucks)}");
            }

            if (missingCustomers.Count > 0)
            {
                problems.Add($"Customers missing: {string.Join(", ", missingCustomers)}");
            }

            return Result<IReadOnlyList<Load>>.Fail(string.Join(" | ", problems));
        }

        // 4) Create domain entities in memory
        var loads = new List<Load>(newLoadList.Count);
        foreach (var newLoad in newLoadList)
        {
            var details = newLoad.Details;
            var dispatcher = dispatcherById[details.AssignedDispatcherId];
            var truck = newLoad.TruckId.HasValue ? truckById[newLoad.TruckId.Value] : null;
            var customer = customerById[details.CustomerId];
            Trip? trip = null;

            if (newLoad.TripId.HasValue)
            {
                trip = tripById[newLoad.TripId.Value];
            }

            var load = Load.Create(
                details.Name,
                details.Type,
                details.DeliveryCost,
                details.OriginAddress,
                details.OriginLocation,
                details.DestinationAddress,
                details.DestinationLocation,
                customer,
                truck,
                dispatcher,
                trip
            );

            load.Distance = details.Distance;
            load.Source = details.Source;
            load.RequestedPickupDate = details.RequestedPickupDate;
            load.RequestedDeliveryDate = details.RequestedDeliveryDate;
            load.PickupAppointmentFrom = details.PickupAppointmentFrom;
            load.PickupAppointmentTo = details.PickupAppointmentTo;
            load.DeliveryAppointmentFrom = details.DeliveryAppointmentFrom;
            load.DeliveryAppointmentTo = details.DeliveryAppointmentTo;
            load.Notes = details.Notes;
            load.ContainerId = details.ContainerId;
            load.OriginTerminalId = details.OriginTerminalId;
            load.DestinationTerminalId = details.DestinationTerminalId;
            load.HazmatClass = details.HazmatClass;
            load.UnNumber = details.UnNumber;
            try
            {
                load.SetVehicles(details.Vehicles.ToLoadVehicles());
            }
            catch (InvalidOperationException ex)
            {
                return Result<IReadOnlyList<Load>>.Fail(ex.Message);
            }

            load.FuelSurcharge = details.FuelSurcharge;
            load.FreightClass = details.FreightClass;
            load.NmfcCode = details.NmfcCode;
            load.PalletCount = details.PalletCount;
            load.PieceCount = details.PieceCount;
            load.Weight = details.Weight;
            load.Dimensions = details.Dimensions;
            load.IsStackable = details.IsStackable;
            load.Accessorials = details.Accessorials.Select(a => LoadChargeService.NewAccessorial(load, a)).ToList();
            if (load.Invoice is { } invoice)
            {
                invoice.ReplaceLoadCharges(load);
                invoice.RecalculateTotals();
            }

            loads.Add(load);
        }

        // 5) Single batched add + single SaveChanges
        await tenantUow.Repository<Load>().AddRangeAsync(loads, ct);

        if (saveChanges)
        {
            await tenantUow.SaveChangesAsync(ct);
        }

        return Result<IReadOnlyList<Load>>.Ok(loads);
    }
}
