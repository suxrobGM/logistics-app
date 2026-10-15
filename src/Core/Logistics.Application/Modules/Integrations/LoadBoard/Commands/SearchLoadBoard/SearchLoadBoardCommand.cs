using Logistics.Application.Attributes;
using Logistics.Domain.Primitives.Enums;
using Logistics.Shared.Models;

namespace Logistics.Application.Modules.Integrations.LoadBoard.Commands;

[RequiresFeature(TenantFeature.LoadBoard)]
public class SearchLoadBoardCommand : ICommand<Result<LoadBoardSearchResultDto>>
{
    public LoadBoardPlace? Origin { get; set; }
    public int OriginRadius { get; set; } = 50;
    public LoadBoardPlace? Destination { get; set; }
    public int DestinationRadius { get; set; } = 50;
    public DateTime? PickupDateStart { get; set; }
    public DateTime? PickupDateEnd { get; set; }
    public LoadBoardEquipmentType[]? EquipmentTypes { get; set; }
    public decimal? MinRatePerMile { get; set; }
    public decimal? MinTotalRate { get; set; }
    public int? MaxWeight { get; set; }
    public int? MaxLength { get; set; }
    public LoadBoardProviderType[]? Providers { get; set; }
    public int MaxResults { get; set; } = 100;
}
