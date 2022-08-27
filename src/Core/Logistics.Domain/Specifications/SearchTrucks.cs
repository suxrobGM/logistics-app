namespace Logistics.Domain.Specifications;

public class SearchTrucks : BaseSpecification<Truck>
{
    public SearchTrucks(string? search, string[] userIds, string[] userNames, string?[] userFirstNames, string?[] userLastNames)
    {
        OrderBy = i => i.TruckNumber;

        if (string.IsNullOrEmpty(search))
            return;
        
        Criteria = i =>
            !string.IsNullOrEmpty(i.DriverId) &&
            userIds.Contains(i.DriverId) &&
            (userNames.Contains(search) || userFirstNames.Contains(search) || userLastNames.Contains(search)) ||

            (i.TruckNumber.ToString().Contains(search, StringComparison.InvariantCultureIgnoreCase));
    }
}