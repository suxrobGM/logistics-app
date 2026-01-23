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
            .Distinct(new LoadComparer())
            .ToArray();
    }

    /// <summary>
    /// Adds new loads to the trip, creating corresponding pick-up and drop-off stops.
    /// </summary>
    /// <param name="loads">The collection of loads to be added.</param>
    /// <exception cref="InvalidOperationException">Thrown if the trip status is not Draft.</exception>
    public void AddLoads(IEnumerable<Load> loads)
    {
        if (!TripStatusMachine.CanModify(Status))
        {
            throw new InvalidOperationException("Cannot modify loads unless trip is Draft.");
        }

        TripFactory.AddStopsForLoads(this, loads);
        TotalDistance = GetLoads().Sum(l => l.Distance);
    }

    /// <summary>
    /// Removes a load from the trip.
    /// </summary>
    /// <param name="loadId">The ID of the load to be removed.</param>
    /// <exception cref="InvalidOperationException">Thrown if the trip status is not Draft.</exception>
    public void RemoveLoad(Guid loadId)
    {
        if (!TripStatusMachine.CanModify(Status))
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
        TotalDistance = GetLoads().Sum(l => l.Distance);
    }

    /// <summary>
    /// Renumbers all stops in sequential order.
    /// </summary>
    private void RenumberStops()
    {
        var order = 1;
        foreach (var s in Stops.OrderBy(x => x.Order))
        {
            s.Order = order++;
        }
    }
}
