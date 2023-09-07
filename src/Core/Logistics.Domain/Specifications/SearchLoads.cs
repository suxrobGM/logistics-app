namespace Logistics.Domain.Specifications;

public class SearchLoads : BaseSpecification<Load>
{
    public SearchLoads(
        string? search,
        string? orderBy = "RefId", 
        bool descending = false)
    {
        if (string.IsNullOrEmpty(search))
            return;
        
        Descending = descending;
        OrderBy = InitOrderBy(orderBy);

        Criteria = i =>
            (i.Name != null && i.Name.Contains(search)) ||
            search.Contains(i.RefId.ToString()) ||
            (i.OriginAddress != null && i.OriginAddress.Contains(search)) || 
            (i.DestinationAddress != null && i.DestinationAddress.Contains(search)) ||
            (i.AssignedTruck != null && search.Contains(i.AssignedTruck.TruckNumber!.ToString()));
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
