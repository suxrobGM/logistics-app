using Logistics.Application.Attributes;
using Logistics.Domain.Primitives.Enums;
using Logistics.Shared.Models;

namespace Logistics.Application.Modules.Integrations.LoadBoard.Commands;

[RequiresFeature(TenantFeature.LoadBoard)]
public class PostTruckToLoadBoardCommand : ICommand<Result<PostTruckResultDto>>
{
    public Guid TruckId { get; set; }
    public LoadBoardProviderType ProviderType { get; set; }

    public required LoadBoardPlace Origin { get; set; }
    public LoadBoardPlace? Destination { get; set; }
    public int? DestinationRadius { get; set; }

    public DateTime AvailableFrom { get; set; }
    public DateTime? AvailableTo { get; set; }

    /// <summary>
    /// Defaults from the truck's type when omitted.
    /// </summary>
    public LoadBoardEquipmentType? EquipmentType { get; set; }

    public int? MaxWeight { get; set; }
    public int? MaxLength { get; set; }
}
