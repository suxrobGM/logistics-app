namespace Logistics.Domain.Specifications;

public class SearchLoads : BaseSpecification<Load>
{
    public SearchLoads(string search, string[] userIds, string[] userNames, string?[] userFirstNames, string?[] userLastNames)
        : base(i =>
            (!string.IsNullOrEmpty(i.Name) &&
             i.Name.Contains(search, StringComparison.InvariantCultureIgnoreCase)) ||
            
            (!string.IsNullOrEmpty(i.AssignedDispatcherId) &&
             userIds.Contains(i.AssignedDispatcherId) &&
             (userNames.Contains(search) || userFirstNames.Contains(search) || userLastNames.Contains(search))) ||
            
            (!string.IsNullOrEmpty(i.AssignedDriverId) &&
             userIds.Contains(i.AssignedDriverId) &&
             (userNames.Contains(search) || userFirstNames.Contains(search) || userLastNames.Contains(search))) ||
            
            (!string.IsNullOrEmpty(i.AssignedDispatcherId) &&
             userIds.Contains(i.AssignedDispatcherId) &&
             (userNames.Contains(search) || userFirstNames.Contains(search) || userLastNames.Contains(search)))
        )
    {
    }
}