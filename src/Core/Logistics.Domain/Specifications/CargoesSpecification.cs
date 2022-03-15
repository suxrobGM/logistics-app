namespace Logistics.Domain.Specifications;

public class CargoesSpecification : BaseSpecification<Cargo>
{
    public CargoesSpecification(string searchInput) 
        : base(i => SearchCriteria(searchInput, i))
    {
        
    }

    private static bool SearchCriteria(string searchInput, Cargo cargo)
    {
        var dispatcherFirstName = false;
        var dispatcherLastName = false;
        var dispatcherUserName = false;
        var driverFirstName = false;
        var driverLastName = false;
        var driverUserName = false;

        if (cargo.AssignedDispatcher != null && 
            !string.IsNullOrEmpty(cargo.AssignedDispatcher.FirstName))
        {
            dispatcherFirstName = cargo.AssignedDispatcher.FirstName.Contains(searchInput, StringComparison.InvariantCultureIgnoreCase);
        }

        if (cargo.AssignedDispatcher != null &&
            !string.IsNullOrEmpty(cargo.AssignedDispatcher.LastName))
        {
            dispatcherLastName = cargo.AssignedDispatcher.LastName.Contains(searchInput, StringComparison.InvariantCultureIgnoreCase);
        }

        if (cargo.AssignedDispatcher != null &&
            !string.IsNullOrEmpty(cargo.AssignedDispatcher.UserName))
        {
            dispatcherUserName = cargo.AssignedDispatcher.UserName.Contains(searchInput, StringComparison.InvariantCultureIgnoreCase);
        }

        if (cargo.AssignedTruck != null &&
            cargo.AssignedTruck.Driver != null &&
            !string.IsNullOrEmpty(cargo.AssignedTruck.Driver.FirstName))
        {
            driverFirstName = cargo.AssignedTruck.Driver.FirstName.Contains(searchInput, StringComparison.InvariantCultureIgnoreCase);
        }

        if (cargo.AssignedTruck != null &&
            cargo.AssignedTruck.Driver != null &&
            !string.IsNullOrEmpty(cargo.AssignedTruck.Driver.LastName))
        {
            driverLastName = cargo.AssignedTruck.Driver.LastName.Contains(searchInput, StringComparison.InvariantCultureIgnoreCase);
        }

        if (cargo.AssignedTruck != null &&
            cargo.AssignedTruck.Driver != null &&
            !string.IsNullOrEmpty(cargo.AssignedTruck.Driver.UserName))
        {
            driverUserName = cargo.AssignedTruck.Driver.UserName.Contains(searchInput, StringComparison.InvariantCultureIgnoreCase);
        }

        return dispatcherFirstName || dispatcherLastName || dispatcherUserName || driverFirstName || driverLastName || driverUserName;
    }
}
