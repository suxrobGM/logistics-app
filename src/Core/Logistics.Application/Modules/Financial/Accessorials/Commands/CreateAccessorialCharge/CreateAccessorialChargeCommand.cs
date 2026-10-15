using Logistics.Domain.Primitives.Enums;
using Logistics.Shared.Models;

namespace Logistics.Application.Modules.Financial.Accessorials.Commands;

public class CreateAccessorialChargeCommand : ICommand<Result<AccessorialChargeDto>>, IAccessorialChargeFields
{
    public string Name { get; set; } = null!;
    public InvoiceLineItemType LineItemType { get; set; } = InvoiceLineItemType.Accessorial;
    public AccessorialUnit Unit { get; set; }
    public decimal DefaultAmount { get; set; }
}
