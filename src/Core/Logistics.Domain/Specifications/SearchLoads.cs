namespace Logistics.Domain.Specifications;

public class SearchLoads : BaseSpecification<Load>
{
    public SearchLoads(
        string? search, 
        string[] userIds, 
        string[] userNames, 
        string?[] userFirstNames,
        string?[] userLastNames,
        string? orderBy = "RefId", 
        bool descending = false)
    {
        Descending = descending;
        OrderBy = InitOrderBy(orderBy);

        if (string.IsNullOrEmpty(search))
            return;

        Criteria = i =>
            (!string.IsNullOrEmpty(i.Name) &&
             i.Name.Contains(search)) ||
            (!string.IsNullOrEmpty(i.AssignedDispatcherId) &&
             userIds.Contains(i.AssignedDispatcherId) &&
             (userNames.Contains(search) || userFirstNames.Contains(search) || userLastNames.Contains(search))) ||
            (!string.IsNullOrEmpty(i.AssignedDriverId) &&
             userIds.Contains(i.AssignedDriverId) &&
             (userNames.Contains(search) || userFirstNames.Contains(search) || userLastNames.Contains(search))) ||
            (!string.IsNullOrEmpty(i.AssignedDispatcherId) &&
             userIds.Contains(i.AssignedDispatcherId) &&
             (userNames.Contains(search) || userFirstNames.Contains(search) || userLastNames.Contains(search)));
    }

    private static Expression<Func<Load, object>> InitOrderBy(string? propertyName)
    {
        propertyName = propertyName?.ToLower() ?? "refid";
        return propertyName switch
        {
            "name" => i => i.Name!,
            "sourceaddress" => i => i.OriginAddress!,
            "destinationaddress" => i => i.DestinationAddress!,
            "deliverycost" => i => i.DeliveryCost,
            "distance" => i => i.Distance,
            "dispatcheddate" => i => i.DispatchedDate,
            "pickupdate" => i => i.PickUpDate!,
            "deliverydate" => i => i.DeliveryDate!,
            "status" => i => i.Status,
            _ => i => i.RefId
        };
    }
}