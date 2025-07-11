using Logistics.Domain.Entities;
using Logistics.Domain.Specifications;

namespace Logistics.Application.Specifications;

public sealed class GetTripsSpec : BaseSpecification<Trip>
{
    public GetTripsSpec(
        string? name,
        string? orderBy,
        int page,
        int pageSize)
    {
        if (!string.IsNullOrEmpty(name))
        {
            Criteria = i => i.Name.Contains(name);
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
