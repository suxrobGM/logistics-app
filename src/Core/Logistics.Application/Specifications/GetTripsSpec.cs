using Logistics.Domain.Entities;
using Logistics.Domain.Primitives.Enums;
using Logistics.Domain.Specifications;

namespace Logistics.Application.Specifications;

public sealed class GetTripsSpec : BaseSpecification<Trip>
{
    public GetTripsSpec(
        string? name,
        TripStatus? status,
        string? truckNumber,
        Guid? truckId,
        DateTime? startDate,
        DateTime? endDate,
        bool onlyActiveTrips,
        string? orderBy,
        int page,
        int pageSize)
    {
        if (!string.IsNullOrEmpty(name))
        {
            Criteria = i => i.Name.Contains(name);
        }

        if (status.HasValue)
        {
            Criteria = Criteria.AndAlso(i => i.Status == status.Value);
        }

        if (!string.IsNullOrEmpty(truckNumber))
        {
            Criteria = Criteria.AndAlso(i => i.Truck != null && i.Truck.Number == truckNumber);
        }

        if (truckId.HasValue)
        {
            Criteria = Criteria.AndAlso(i => i.TruckId == truckId.Value);
        }

        if (startDate.HasValue && endDate.HasValue)
        {
            Criteria = Criteria.AndAlso(i => i.CreatedAt >= startDate.Value && i.CreatedAt <= endDate.Value);
        }

        if (onlyActiveTrips)
        {
            Criteria = Criteria.AndAlso(i => i.Status != TripStatus.Completed && i.Status != TripStatus.Cancelled);
        }

        OrderBy(orderBy);
        ApplyPaging(page, pageSize);
    }

    // protected override Expression<Func<Trip, object?>> CreateOrderByExpression(string propertyName)
    // {
    //     return propertyName switch
    //     {
    //         "actualstart" => i => i.ActualStart,
    //         "completedat" => i => i.CompletedAt,
    //         "name" => i => i.Name,
    //         "destinationaddress" => i => i.DestinationAddress.ToString(),
    //         "number" => i => i.Number,
    //         "originaddress" => i => i.OriginAddress.ToString(),
    //         "totaldistance" => i => i.TotalDistance,
    //         _ => i => i.Number
    //     };
    // }
}
