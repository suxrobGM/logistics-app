using Logistics.Domain.Primitives.Enums;
using Logistics.Domain.Primitives.ValueObjects;

namespace Logistics.Shared.Models;

public record PostedTruckDto
{
    public Guid Id { get; set; }
    public Guid TruckId { get; set; }
    public string? TruckNumber { get; set; }
    public LoadBoardProviderType ProviderType { get; set; }
    public string? ProviderName { get; set; }
    public string? ExternalPostId { get; set; }

    public required Address AvailableAtAddress { get; set; }
    public required GeoPoint AvailableAtLocation { get; set; }

    public Address? DestinationPreference { get; set; }
    public int? DestinationRadius { get; set; }

    public DateTime AvailableFrom { get; set; }
    public DateTime? AvailableTo { get; set; }

    public LoadBoardEquipmentType? EquipmentType { get; set; }
    public int? MaxWeight { get; set; }
    public int? MaxLength { get; set; }

    public PostedTruckStatus Status { get; set; }
    public DateTime? ExpiresAt { get; set; }
    public DateTime? LastRefreshedAt { get; set; }
    public DateTime CreatedAt { get; set; }
}

public record PostTruckRequest
{
    public Guid TruckId { get; set; }

    public required LoadBoardPlace Origin { get; set; }

    /// <summary>
    /// Preferred destination; null posts the truck as open to anywhere.
    /// </summary>
    public LoadBoardPlace? Destination { get; set; }

    public int? DestinationRadius { get; set; }

    public DateTime AvailableFrom { get; set; }
    public DateTime? AvailableTo { get; set; }

    public required LoadBoardEquipmentType EquipmentType { get; set; }
    public int? MaxWeight { get; set; }
    public int? MaxLength { get; set; }
}

public record PostTruckResultDto
{
    public required string ExternalPostId { get; set; }
    public Guid? PostedTruckId { get; set; }
    public DateTime? ExpiresAt { get; set; }
}
