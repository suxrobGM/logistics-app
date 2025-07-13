using Logistics.Domain.Entities;
using Logistics.Domain.Specifications;
using Logistics.Shared.Consts;

namespace Logistics.Application.Specifications;

public sealed class GetTripsSpec : BaseSpecification<Trip>
{
    public GetTripsSpec(
        string? name,
        TripStatus? status,
        string? truckNumber,
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
            Criteria = Criteria.AndAlso(i => i.Truck.Number == truckNumber);
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
