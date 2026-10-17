using Logistics.Application.Attributes;
using Logistics.Domain.Primitives.Enums;
using Logistics.Shared.Models;

namespace Logistics.Application.Modules.Integrations.FuelCards.Commands;

/// <summary>
/// Dismisses pending fuel card transactions (e.g. cash advances or non-fleet purchases)
/// so they leave the review queue without creating an expense. Matched transactions are skipped.
/// </summary>
[RequiresFeature(TenantFeature.FuelCards)]
public class IgnoreFuelCardTransactionCommand : ICommand<Result>
{
    public required List<Guid> TransactionIds { get; set; }
}
