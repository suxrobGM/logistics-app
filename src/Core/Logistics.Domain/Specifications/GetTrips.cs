using System.Linq.Expressions;
using Logistics.Domain.Entities;

namespace Logistics.Domain.Specifications;

public sealed class GetTrips : BaseSpecification<Trip>
{
    public GetTrips(
        string? name,
        string? orderBy,
        int page,
        int pageSize)
    {
        if (!string.IsNullOrEmpty(name))
        {
            Criteria = i => i.Name.Contains(name);
        }
        
        ApplyOrderBy(orderBy);
        ApplyPaging(page, pageSize);
    }
    
    protected override Expression<Func<Trip, object?>> CreateOrderByExpression(string propertyName)
    {
        return propertyName switch
        {
            "actualstart" => i => i.ActualStart,
            "completedat" => i => i.CompletedAt,
            "name" => i => i.Name,
            "destinationaddress" => i => i.DestinationAddress.ToString(),
            "number" => i => i.Number,
            "originaddress" => i => i.OriginAddress.ToString(),
            "totaldistance" => i => i.TotalDistance,
            _ => i => i.Number
        };
    }
}
