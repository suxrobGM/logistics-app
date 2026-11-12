using Logistics.Domain.Entities;

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
    }
}
