using Logistics.Application.Abstractions;
using Logistics.Application.Attributes;
using Logistics.Domain.Primitives.Enums;
using Logistics.Shared.Models;

namespace Logistics.Application.Modules.Integrations.FuelCards.Commands;

/// <summary>
/// Sets or clears the truck a fuel card is mapped to.
/// </summary>
[RequiresFeature(TenantFeature.FuelCards)]
public class UpdateFuelCardMappingCommand : ICommand<Result>
{
    public Guid FuelCardId { get; set; }

    /// <summary>Null unassigns the card.</summary>
    public Guid? TruckId { get; set; }
}
