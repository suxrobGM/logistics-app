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

    public async Task<Load> CreateLoadAsync(CreateLoadParameters parameters, bool saveChanges = true,
        CancellationToken ct = default)
    {
        var created = await CreateLoadsAsync([parameters], saveChanges, ct);
        return created.First();
    }

    public async Task<IReadOnlyCollection<Load>> CreateLoadsAsync(IEnumerable<CreateLoadParameters> parameters,
        bool saveChanges = true, CancellationToken ct = default)
    {
        var paramList = parameters as IList<CreateLoadParameters> ?? parameters.ToList();
        if (paramList.Count == 0)
        {
            return [];
        }

        // 1) Collect distinct foreign keys
        var dispatcherIds = paramList.Select(p => p.DispatcherId).Distinct().ToList();
        var truckIds = paramList.Select(p => p.TruckId).Distinct().ToList();
        var customerIds = paramList.Select(p => p.CustomerId).Distinct().ToList();
        var tripIds = paramList.Select(p => p.TripId).Distinct().ToList();

        // 2) Batch-load referenced rows (at most 3 queries total)
        var dispatcherRepo = _tenantUow.Repository<Employee>();
        var truckRepo = _tenantUow.Repository<Truck>();
        var customerRepo = _tenantUow.Repository<Customer>();
        var tripRepo = _tenantUow.Repository<Trip>();

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

            throw new InvalidOperationException(string.Join(" | ", problems));
        }

        // 4) Create domain entities in memory
        var loads = new List<Load>(paramList.Count);
        foreach (var p in paramList)
        {
            var dispatcher = dispatcherById[p.DispatcherId];
            var truck = truckById[p.TruckId];
            var customer = customerById[p.CustomerId];
            Trip? trip = null;

            if (p.TripId.HasValue)
            {
                trip = tripById[p.TripId.Value];
            }

            var load = Load.Create(
                p.Name,
                p.Type,
                p.DeliveryCost,
                p.Origin.address,
                p.Origin.location,
                p.Destination.address,
                p.Destination.location,
                customer,
                truck,
                dispatcher,
                trip
            );

            load.Distance = p.Distance;
            loads.Add(load);
        }

        // 5) Single batched add + single SaveChanges
        await _tenantUow.Repository<Load>().AddRangeAsync(loads, ct);

        if (saveChanges)
        {
            await _tenantUow.SaveChangesAsync(ct);
        }

        return loads;
    }

    public async Task DeleteLoadAsync(Guid loadId)
    {
        var load = await _tenantUow.Repository<Load>().GetByIdAsync(loadId);
        _tenantUow.Repository<Load>().Delete(load);
        await _tenantUow.SaveChangesAsync();
    }
}
