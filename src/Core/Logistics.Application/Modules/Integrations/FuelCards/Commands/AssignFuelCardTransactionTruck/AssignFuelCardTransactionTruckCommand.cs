using Logistics.Application.Abstractions;
using Logistics.Application.Attributes;
using Logistics.Domain.Primitives.Enums;
using Logistics.Shared.Models;

namespace Logistics.Application.Modules.Integrations.FuelCards.Commands;

/// <summary>
/// Assigns a pending fuel card transaction to a truck, materializing it as a fuel expense.
/// </summary>
[RequiresFeature(TenantFeature.FuelCards)]
public class AssignFuelCardTransactionTruckCommand : ICommand<Result>
{
    public Guid TransactionId { get; set; }
    public Guid TruckId { get; set; }

    /// <summary>
    /// Also remember the card → truck mapping so future transactions on this card auto-match.
    /// </summary>
    public bool RememberMapping { get; set; }
}
