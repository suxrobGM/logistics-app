namespace Logistics.Domain.Specifications;

public class SearchTrucks : BaseSpecification<Truck>
{
    public SearchTrucks(string search, string[] userIds, string[] userNames, string?[] userFirstNames, string?[] userLastNames)
    {
        Criteria = i =>
            !string.IsNullOrEmpty(i.DriverId) &&
            userIds.Contains(i.DriverId) &&
            (userNames.Contains(search) || userFirstNames.Contains(search) || userLastNames.Contains(search)) ||

            (i.TruckNumber.HasValue &&
             i.TruckNumber.Value.ToString().Contains(search, StringComparison.InvariantCultureIgnoreCase));
    }
}