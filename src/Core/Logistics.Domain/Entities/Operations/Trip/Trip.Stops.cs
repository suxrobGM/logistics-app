namespace Logistics.Domain.Entities;

/// <summary>
/// Stop management methods for Trip entity.
/// </summary>
public partial class Trip
{
    /// <summary>
    /// Gets all unique loads associated with the trip.
    /// </summary>
    public IReadOnlyList<Load> GetLoads()
    {
        return Stops
            .Select(s => s.Load)
            .Where(i => i is not null)
            .DistinctBy(l => l.Id)
            .ToArray();
    }

    /// <summary>Adds a pickup and drop-off stop per load and gives the loads the trip's truck.</summary>
    /// <exception cref="InvalidOperationException">The trip is not Draft.</exception>
    public void AddLoads(IEnumerable<Load> loads)
    {
        if (!CanModify)
        {
            throw new InvalidOperationException("Cannot modify loads unless trip is Draft.");
        }

        var loadsArr = loads.ToArray();
        if (TruckId is not null)
        {
            foreach (var load in loadsArr)
            {
                load.AssignedTruckId = TruckId;
                load.AssignedTruck = Truck;
            }
        }

        AddStopsForLoads(loadsArr);
        RecalculateDistance();
    }

    /// <exception cref="InvalidOperationException">The trip is not Draft.</exception>
    public void RemoveLoad(Guid loadId)
    {
        if (!CanModify)
        {
            throw new InvalidOperationException("Cannot modify loads unless trip is Draft.");
        }

        var toRemove = Stops.Where(s => s.LoadId == loadId).ToList();
        if (toRemove.Count == 0)
        {
            return;
        }

        foreach (var s in toRemove)
        {
            Stops.Remove(s);
        }

        RenumberStops();
        RecalculateDistance();
    }

    /// <summary>Re-sums <see cref="TotalDistance"/> from the trip's loads.</summary>
    public void RecalculateDistance()
    {
        TotalDistance = GetLoads().Sum(l => l.Distance);
    }

    /// <summary>
    /// Renumbers all stops in sequential order.
    /// </summary>
    private void RenumberStops()
    {
        var order = 1;
        foreach (var s in Stops.OrderBy(x => x.Position))
        {
            s.Position = order++;
        }
    }
}
