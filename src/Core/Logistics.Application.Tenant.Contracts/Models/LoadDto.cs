using Logistics.Domain.Shared;

namespace Logistics.Application.Contracts.Models;

public class LoadDto
{
    public string? Id { get; set; }

    public string? Name { get; set; }
    
    [Required]
    public string? SourceAddress { get; set; }

    [Required]
    public string? DestinationAddress { get; set; }

    [Required]
    [Range(CargoConsts.MinPricePerMile, CargoConsts.MaxPricePerMile)]
    public decimal PricePerMile { get; set; }

    [Required]
    [Range(CargoConsts.MinTripMiles, CargoConsts.MaxTripMiles)]
    public double TotalTripMiles { get; set; }

    public bool IsCompleted { get; set; }
    public DateTime PickUpDate { get; set; } = DateTime.Now;
    public string? Status { get; set; }

    [Required]
    public string? AssignedDispatcherId { get; set; }
    public string? AssignedDispatcherName { get; set; }

    [Required]
    public string? AssignedTruckId { get; set; }
    public string? AssignedTruckDriverName { get; set; }
}
