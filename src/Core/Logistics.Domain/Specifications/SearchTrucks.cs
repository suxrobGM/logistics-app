namespace Logistics.Domain.Specifications;

public class SearchTrucks : BaseSpecification<Truck>
{
    public SearchTrucks(
        string? search,
        string? orderBy,
        bool descending = false)
    {
        OrderBy = InitOrderBy(orderBy);
        Descending = descending;

        if (string.IsNullOrEmpty(search))
            return;
        
        Criteria = i => 
            i.TruckNumber != null && i.TruckNumber.ToString().Contains(search);
    }
    
    private static Expression<Func<Truck, object>> InitOrderBy(string? propertyName)
    {
        propertyName = propertyName?.ToLower() ?? string.Empty;
        return propertyName switch
        {
            "driverincomepercentage" => i => i.DriverIncomePercentage!,
            _ => i => i.TruckNumber!
        };
    }
}
