using Logistics.Application.Attributes;
using Logistics.Domain.Primitives.Enums;
using Logistics.Shared.Models;

namespace Logistics.Application.Modules.Integrations.FuelCards.Commands;

/// <summary>
/// Assigns pending fuel card transactions to a truck, materializing each as a fuel expense.
/// Transactions that are already matched are skipped.
/// </summary>
[RequiresFeature(TenantFeature.FuelCards)]
public class AssignFuelCardTransactionTruckCommand : ICommand<Result>
{
    public required List<Guid> TransactionIds { get; set; }
    public Guid TruckId { get; set; }

    /// <summary>
    /// Also remember the card → truck mapping so future transactions on this card auto-match.
    /// </summary>
    public bool RememberMapping { get; set; }
}
