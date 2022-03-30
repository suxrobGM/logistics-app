namespace Logistics.Domain.Specifications;

public class TrucksSpecification : BaseSpecification<Truck>
{
    public TrucksSpecification(string searchInput) 
        : base(i =>
            (i.Driver != null && 
            !string.IsNullOrEmpty(i.Driver.FirstName) && 
            i.Driver.FirstName.Contains(searchInput, StringComparison.InvariantCultureIgnoreCase)) ||
            
            (i.Driver != null &&
            !string.IsNullOrEmpty(i.Driver.LastName) &&
            i.Driver.LastName.Contains(searchInput, StringComparison.InvariantCultureIgnoreCase)) ||

            (i.TruckNumber.HasValue &&
            i.TruckNumber.Value.ToString().Contains(searchInput, StringComparison.InvariantCultureIgnoreCase))
        )
    {
    }

    private static bool SearchCriteria(string searchInput, Truck truck)
    {
        var driverFirstName = false;
        var driverLastName = false;
        var driverUserName = false;
        var truckNumber = false;

        if (truck.Driver != null && 
            !string.IsNullOrEmpty(truck.Driver.FirstName))
        {
            driverFirstName = truck.Driver.FirstName.Contains(searchInput, StringComparison.InvariantCultureIgnoreCase);
        }

        if (truck.Driver != null &&
            !string.IsNullOrEmpty(truck.Driver.LastName))
        {
            driverLastName = truck.Driver.LastName.Contains(searchInput, StringComparison.InvariantCultureIgnoreCase);
        }

        if (truck.Driver != null &&
            !string.IsNullOrEmpty(truck.Driver.UserName))
        {
            driverUserName = truck.Driver.UserName.Contains(searchInput, StringComparison.InvariantCultureIgnoreCase);
        }

        if (truck.TruckNumber.HasValue)
        {
            truckNumber = truck.TruckNumber.Value.ToString().Contains(searchInput, StringComparison.InvariantCultureIgnoreCase);
        }

        return driverFirstName || driverLastName || driverUserName || truckNumber;
    }
}
