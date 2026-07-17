using Logistics.Application.Abstractions;
using Logistics.Application.Attributes;
using Logistics.Domain.Primitives.Enums;
using Logistics.Shared.Models;

namespace Logistics.Application.Modules.Integrations.FuelCards.Commands;

/// <summary>
/// Dismisses a pending fuel card transaction (e.g. cash advance or non-fleet purchase)
/// so it leaves the review queue without creating an expense.
/// </summary>
[RequiresFeature(TenantFeature.FuelCards)]
public class IgnoreFuelCardTransactionCommand : ICommand<Result>
{
    public Guid TransactionId { get; set; }
}
