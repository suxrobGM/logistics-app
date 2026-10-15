using Logistics.Application.Attributes;
using Logistics.Domain.Primitives.Enums;
using Logistics.Shared.Models;

namespace Logistics.Application.Modules.Integrations.LoadBoard.Commands;

/// <summary>
/// Replaces an active truck post's lane, window and capacity on the board and locally.
/// </summary>
[RequiresFeature(TenantFeature.LoadBoard)]
public class UpdatePostedTruckCommand : ICommand
{
    public Guid PostedTruckId { get; set; }

    public required LoadBoardPlace Origin { get; set; }
    public LoadBoardPlace? Destination { get; set; }
    public int? DestinationRadius { get; set; }

    public DateTime AvailableFrom { get; set; }
    public DateTime? AvailableTo { get; set; }

    public required LoadBoardEquipmentType EquipmentType { get; set; }
    public int? MaxWeight { get; set; }
    public int? MaxLength { get; set; }
}
