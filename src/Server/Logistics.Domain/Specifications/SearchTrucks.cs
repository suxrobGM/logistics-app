using System.Linq.Expressions;
using Logistics.Domain.Entities;

namespace Logistics.Domain.Specifications;

public sealed class SearchTrucks : BaseSpecification<Truck>
{
    public SearchTrucks(
        string? search,
        string? orderBy,
        int page,
        int pageSize)
    {
        if (!string.IsNullOrEmpty(search))
        {
            Criteria = i => i.TruckNumber.Contains(search);
        }
        
        ApplyOrderBy(orderBy);
        ApplyPaging(page, pageSize);
    }
    
    protected override Expression<Func<Truck, object?>> CreateOrderByExpression(string propertyName)
    {
        return propertyName switch
        {
            // "driverincomepercentage" => i => i.DriverIncomePercentage,
            _ => i => i.TruckNumber
        };
    }
}
