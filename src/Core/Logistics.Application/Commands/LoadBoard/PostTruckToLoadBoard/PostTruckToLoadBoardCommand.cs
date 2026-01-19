using Logistics.Application.Abstractions;
using Logistics.Domain.Primitives.Enums;
using Logistics.Domain.Primitives.ValueObjects;
using Logistics.Shared.Models;

namespace Logistics.Application.Commands;

public class PostTruckToLoadBoardCommand : IAppRequest<Result<PostTruckResultDto>>
{
    public Guid TruckId { get; set; }
    public LoadBoardProviderType ProviderType { get; set; }

    public required Address AvailableAtAddress { get; set; }
    public required GeoPoint AvailableAtLocation { get; set; }

    public Address? DestinationPreference { get; set; }
    public int? DestinationRadius { get; set; }

    public DateTime AvailableFrom { get; set; }
    public DateTime? AvailableTo { get; set; }

    public string? EquipmentType { get; set; }
    public int? MaxWeight { get; set; }
    public int? MaxLength { get; set; }
}
