using Logistics.Application.Modules.Operations.Loads.Queries;
using Logistics.Domain.Entities;
using Logistics.Domain.Primitives.Enums;

namespace Logistics.Application.Modules.Operations.Loads;

public static class LoadQueryableExtensions
{
    extension(IQueryable<Load> loads)
    {
        /// <summary>Loads delivered inside the period, optionally only those hauled by one truck.</summary>
        public IQueryable<Load> FilterDeliveredBetween(Guid? truckId, DateTime start, DateTime end)
        {
            var delivered = loads.Where(i => i.DeliveredAt.HasValue && i.DeliveredAt >= start && i.DeliveredAt <= end);
            return truckId.HasValue ? delivered.Where(i => i.AssignedTruckId == truckId) : delivered;
        }

        /// <summary>Applies the load list filters; <paramref name="driverId"/> limits to loads on that driver's truck.</summary>
        public IQueryable<Load> FilterForList(ILoadListFilter filter, Guid? driverId)
        {
            var query = loads;

            if (!string.IsNullOrEmpty(filter.Search))
            {
                var search = filter.Search.ToLower();
                query = query.Where(i =>
                    (i.Name != null && i.Name.ToLower().Contains(search)) ||
                    (i.Customer != null && i.Customer.Name.ToLower().Contains(search)) ||
                    i.Number.ToString().Contains(search) ||
                    i.OriginAddress.Line1.ToLower().Contains(search) ||
                    (i.OriginAddress.Line2 != null && i.OriginAddress.Line2.ToLower().Contains(search)) ||
                    i.DestinationAddress.Line1.ToLower().Contains(search) ||
                    (i.DestinationAddress.Line2 != null && i.DestinationAddress.Line2.ToLower().Contains(search)));
            }

            if (filter.ActiveOnly)
            {
                query = query.Where(i => i.Status == LoadStatus.Dispatched || i.Status == LoadStatus.PickedUp);
            }

            if (filter.Unassigned)
            {
                query = query.Where(i => i.AssignedTruckId == null);
            }

            if (driverId.HasValue)
            {
                query = query.Where(i => i.AssignedTruck != null &&
                                         (i.AssignedTruck.MainDriverId == driverId ||
                                          i.AssignedTruck.SecondaryDriverId == driverId));
            }

            if (filter.TruckId.HasValue)
            {
                query = query.Where(i => i.AssignedTruckId == filter.TruckId);
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

            if (filter.CustomerId.HasValue)
            {
                query = query.Where(i => i.CustomerId == filter.CustomerId.Value);
            }

            if (filter.FromDate is { } fromDate)
            {
                query = query.Where(i => i.DispatchedAt >= fromDate);
            }

            if (filter.ToDate is { } toDate)
            {
                query = query.Where(i => i.DispatchedAt <= toDate);
            }

            return query;
        }
    }
}
