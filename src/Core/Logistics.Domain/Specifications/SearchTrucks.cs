using System.Linq.Expressions;
using Logistics.Domain.Entities;

namespace Logistics.Domain.Specifications;

public sealed class SearchTrucks : BaseSpecification<Truck>
{
    public SearchTrucks(
        string? search,
        string? orderBy,
        int page,
        int pageSize,
        bool descending)
    {
        if (!string.IsNullOrEmpty(search))
        {
            Criteria = i => i.TruckNumber.Contains(search);
        }
        
        ApplyOrderBy(InitOrderBy(orderBy), descending);
        ApplyPaging(page, pageSize);
    }
    
    private static Expression<Func<Truck, object?>> InitOrderBy(string? propertyName)
    {
        propertyName = propertyName?.ToLower() ?? string.Empty;
        return propertyName switch
        {
            // "driverincomepercentage" => i => i.DriverIncomePercentage,
            _ => i => i.TruckNumber
        };
    }
}
