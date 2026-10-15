using Logistics.Domain.Primitives.Enums;
using Logistics.Shared.Models;

namespace Logistics.Application.Modules.Financial.Accessorials.Commands;

/// <summary>Full replace. Loads that already carry the charge keep the values they were given.</summary>
public class UpdateAccessorialChargeCommand : ICommand<Result>, IAccessorialChargeFields
{
    public Guid Id { get; set; }
    public string Name { get; set; } = null!;
    public InvoiceLineItemType LineItemType { get; set; }
    public AccessorialUnit Unit { get; set; }
    public decimal DefaultAmount { get; set; }
}
