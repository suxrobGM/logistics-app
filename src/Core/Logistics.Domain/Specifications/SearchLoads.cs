using System.Linq.Expressions;
using Logistics.Domain.Entities;

namespace Logistics.Domain.Specifications;

public class SearchLoads : BaseSpecification<Load>
{
    public SearchLoads(
        string? search,
        string? orderBy, 
        bool descending = false)
    {
        Descending = descending;
        OrderBy = InitOrderBy(orderBy);
        
        if (string.IsNullOrEmpty(search))
            return;

        Criteria = i =>
            (i.Name != null && i.Name.Contains(search)) ||
            i.RefId.ToString().Contains(search) ||
            i.OriginAddress.Contains(search) || 
            i.DestinationAddress.Contains(search);
    }

    private static Expression<Func<Load, object?>> InitOrderBy(string? propertyName)
    {
        propertyName = propertyName?.ToLower() ?? string.Empty;
        return propertyName switch
        {
            "name" => i => i.Name,
            "originaddress" => i => i.OriginAddress,
            "destinationaddress" => i => i.DestinationAddress,
            "deliverycost" => i => i.DeliveryCost,
            "distance" => i => i.Distance,
            "dispatcheddate" => i => i.DispatchedDate,
            "pickupdate" => i => i.PickUpDate,
            "deliverydate" => i => i.DeliveryDate,
            "invoicepaymentstatus" => i => i.Invoice!.Payment.Status,
            "assignedtruck" => i => i.AssignedTruck!.TruckNumber,
            "assigneddispatcher" => i => i.AssignedDispatcher!.FirstName,
            _ => i => i.RefId
        };
    }
}
