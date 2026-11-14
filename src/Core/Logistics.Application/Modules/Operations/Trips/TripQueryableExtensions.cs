using Logistics.Application.Modules.Operations.Trips.Queries;
using Logistics.Domain.Entities;
using Logistics.Domain.Primitives.Enums;

namespace Logistics.Application.Modules.Operations.Trips;

public static class TripQueryableExtensions
{
    extension(IQueryable<Trip> trips)
    {
        public IQueryable<Trip> FilterForList(ITripListFilter filter)
        {
            var query = trips;

            if (!string.IsNullOrEmpty(filter.Search))
            {
                var search = filter.Search.ToLower();
                query = query.Where(i =>
                    i.Name.ToLower().Contains(search) ||
                    i.Number.ToString().Contains(search) ||
                    (i.Truck != null && i.Truck.Number.ToLower().Contains(search)));
            }

            if (filter.Statuses?.Length > 0)
            {
                var statuses = filter.Statuses;
                query = query.Where(i => statuses.Contains(i.Status));
            }

            if (filter.TruckId.HasValue)
            {
                query = query.Where(i => i.TruckId == filter.TruckId.Value);
            }

            if (filter.FromDate is { } fromDate)
            {
                query = query.Where(i => i.CreatedAt >= fromDate);
            }

            if (filter.ToDate is { } toDate)
            {
                query = query.Where(i => i.CreatedAt <= toDate);
            }

            if (filter.ActiveOnly)
            {
                query = query.Where(i => i.Status != TripStatus.Completed && i.Status != TripStatus.Cancelled);
            }

            return query;
        }
    }
}
