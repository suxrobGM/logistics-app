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
}
