using Logistics.Domain.Primitives.Enums;

namespace Logistics.Application.Modules.Financial.Accessorials.Commands;

public interface IAccessorialChargeFields
{
    string Name { get; }
    InvoiceLineItemType LineItemType { get; }
    AccessorialUnit Unit { get; }
    decimal DefaultAmount { get; }
}
