using Logistics.Application.Abstractions;
using Logistics.Domain.Primitives.Enums;
using Logistics.Domain.Primitives.ValueObjects;
using Logistics.Shared.Models;

namespace Logistics.Application.Commands;

public class SearchLoadBoardCommand : IAppRequest<Result<LoadBoardSearchResultDto>>
{
    public Address? OriginAddress { get; set; }
    public int OriginRadius { get; set; } = 50;
    public Address? DestinationAddress { get; set; }
    public int DestinationRadius { get; set; } = 50;
    public DateTime? PickupDateStart { get; set; }
    public DateTime? PickupDateEnd { get; set; }
    public string[]? EquipmentTypes { get; set; }
    public decimal? MinRatePerMile { get; set; }
    public decimal? MinTotalRate { get; set; }
    public int? MinWeight { get; set; }
    public int? MaxWeight { get; set; }
    public int? MaxLength { get; set; }
    public LoadBoardProviderType[]? Providers { get; set; }
    public int MaxResults { get; set; } = 100;
}
