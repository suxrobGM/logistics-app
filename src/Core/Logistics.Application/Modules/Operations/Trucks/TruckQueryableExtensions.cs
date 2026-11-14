using Logistics.Application.Modules.Operations.Trucks.Queries;
using Logistics.Domain.Entities;
using Logistics.Domain.Primitives.Enums;

namespace Logistics.Application.Modules.Operations.Trucks;

public static class TruckQueryableExtensions
{
    extension(IQueryable<Truck> trucks)
    {
        public IQueryable<Truck> FilterForList(ITruckListFilter filter)
        {
            var query = trucks;

            if (!string.IsNullOrEmpty(filter.Search))
            {
                var search = filter.Search.ToLower();
                query = query.Where(i => i.Number.ToLower().Contains(search) ||
                    (i.MainDriver != null && (i.MainDriver.FirstName.ToLower().Contains(search) ||
                                              i.MainDriver.LastName.ToLower().Contains(search))) ||
                    (i.SecondaryDriver != null &&
                     (i.SecondaryDriver.FirstName.ToLower().Contains(search) ||
                      i.SecondaryDriver.LastName.ToLower().Contains(search))));
            }

            if (filter.Statuses?.Length > 0)
            {
                var statuses = filter.Statuses;
                query = query.Where(i => statuses.Contains(i.Status));
            }

            if (filter.Types?.Length > 0)
            {
                var types = filter.Types;
                query = query.Where(i => types.Contains(i.Type));
            }

            if (filter.Idle)
            {
                // Same rule as the dashboard's idle count: no load out on the road.
                query = query.Where(i => !i.Loads.Any(l =>
                    l.Status == LoadStatus.Dispatched || l.Status == LoadStatus.PickedUp));
            }

            return query;
        }
    }
}
