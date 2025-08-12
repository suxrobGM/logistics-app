using Logistics.Domain.Entities;
using Logistics.Domain.Specifications;

namespace Logistics.Application.Specifications;

public class SearchLoads : BaseSpecification<Load>
{
    public SearchLoads(string? search, string? orderBy)
    {
        if (!string.IsNullOrEmpty(search))
        {
            Criteria = i =>
                (i.Name != null && i.Name.Contains(search)) ||
                (i.Customer != null && i.Customer.Name.Contains(search)) ||
                i.Number.ToString().Contains(search) ||
                i.OriginAddress.Line1.Contains(search) ||
                (i.OriginAddress.Line2 != null && i.OriginAddress.Line2.Contains(search)) ||
                i.DestinationAddress.Line1.Contains(search) ||
                (i.DestinationAddress.Line2 != null && i.DestinationAddress.Line2.Contains(search));
        }

        OrderBy(orderBy);
    }

    // protected override Expression<Func<Load, object?>> CreateOrderByExpression(string propertyName)
    // {
    //     return propertyName switch
    //     {
    //         "name" => i => i.Name,
    //         "customer" => i => i.Customer!.Name,
    //         "originaddress" => i => i.OriginAddress.Line1,
    //         "destinationaddress" => i => i.DestinationAddress.Line1,
    //         "deliverycost" => i => i.DeliveryCost,
    //         "distance" => i => i.Distance,
    //         "dispatcheddate" => i => i.DispatchedDate,
    //         "pickupdate" => i => i.PickUpDate,
    //         "deliverydate" => i => i.DeliveryDate,
    //         "assignedtruck" => i => i.AssignedTruck!.TruckNumber,
    //         "assigneddispatcher" => i => i.AssignedDispatcher!.FirstName,
    //         _ => i => i.Number
    //     };
    // }
}
