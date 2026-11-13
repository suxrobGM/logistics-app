using Logistics.Application.Common.Pipeline;
using Logistics.Domain.Primitives.Enums;
using Logistics.Shared.Models;

namespace Logistics.Application.Modules.Integrations.LoadBoard.Commands;

[RequiresFeature(TenantFeature.LoadBoard)]
public class SearchLoadBoardCommand : ICommand<Result<LoadBoardSearchResultDto>>
{
    public LoadBoardPlace? Origin { get; set; }
    public int OriginRadiusMeters { get; set; } = 80_000;
    public LoadBoardPlace? Destination { get; set; }
    public int DestinationRadiusMeters { get; set; } = 80_000;
    public DateTime? PickupDateStart { get; set; }
    public DateTime? PickupDateEnd { get; set; }
    public LoadBoardEquipmentType[]? EquipmentTypes { get; set; }
    public decimal? MinRatePerKm { get; set; }
    public decimal? MinTotalRate { get; set; }
    public decimal? MaxWeightKg { get; set; }
    public decimal? MaxLengthMeters { get; set; }
    public LoadBoardProviderType[]? Providers { get; set; }
    public int MaxResults { get; set; } = 100;
}
