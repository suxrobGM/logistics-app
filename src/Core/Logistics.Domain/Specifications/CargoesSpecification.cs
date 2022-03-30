namespace Logistics.Domain.Specifications;

public class CargoesSpecification : BaseSpecification<Cargo>
{
    public CargoesSpecification(string searchInput) 
        : base(i => 
            (i.AssignedDispatcher != null &&
            !string.IsNullOrEmpty(i.AssignedDispatcher.FirstName) &&
            !i.AssignedDispatcher.FirstName.Contains(searchInput, StringComparison.InvariantCultureIgnoreCase)) ||

            (i.AssignedDispatcher != null &&
            !string.IsNullOrEmpty(i.AssignedDispatcher.LastName) &&
            !i.AssignedDispatcher.LastName.Contains(searchInput, StringComparison.InvariantCultureIgnoreCase)) ||
        
            (i.AssignedDispatcher != null &&
            !string.IsNullOrEmpty(i.AssignedDispatcher.UserName) &&
            !i.AssignedDispatcher.UserName.Contains(searchInput, StringComparison.InvariantCultureIgnoreCase)) ||

            (i.AssignedTruck != null &&
            i.AssignedTruck.Driver != null &&
            !string.IsNullOrEmpty(i.AssignedTruck.Driver.FirstName) &&
            !i.AssignedTruck.Driver.FirstName.Contains(searchInput, StringComparison.InvariantCultureIgnoreCase)) ||

            (i.AssignedTruck != null &&
            i.AssignedTruck.Driver != null &&
            !string.IsNullOrEmpty(i.AssignedTruck.Driver.LastName) &&
            !i.AssignedTruck.Driver.LastName.Contains(searchInput, StringComparison.InvariantCultureIgnoreCase)) ||

            (i.AssignedTruck != null &&
            i.AssignedTruck.Driver != null &&
            !string.IsNullOrEmpty(i.AssignedTruck.Driver.UserName) &&
            !i.AssignedTruck.Driver.UserName.Contains(searchInput, StringComparison.InvariantCultureIgnoreCase))
        )
    {
    }
}
