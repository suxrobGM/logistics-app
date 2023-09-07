namespace Logistics.Domain.Specifications;

public class SearchTrucks : BaseSpecification<Truck>
{
    public SearchTrucks(
        string? search,
        bool descending = false)
    {
        OrderBy = i => i.TruckNumber!;
        Descending = descending;

        if (string.IsNullOrEmpty(search))
            return;
        
        Criteria = i => 
            i.TruckNumber!.ToString().Contains(search);
    }
}
