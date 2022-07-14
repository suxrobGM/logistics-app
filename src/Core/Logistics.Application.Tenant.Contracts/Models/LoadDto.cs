using Logistics.Domain.Shared;

namespace Logistics.Application.Contracts.Models;

public class LoadDto
{
    public string? Id { get; set; }
    public ulong ReferenceId { get; set; } = 100_000;

    public string? Name { get; set; }
    
    [Required]
    public string? SourceAddress { get; set; }

    [Required]
    public string? DestinationAddress { get; set; }

    [Required]
    [Range(LoadConsts.MinDeliveryCost, LoadConsts.MinDeliveryCost)]
    public decimal DeliveryCost { get; set; }

    [Required]
    [Range(LoadConsts.MinTripMiles, LoadConsts.MaxTripMiles)]
    public double TotalTripMiles { get; set; }
    
    public DateTime PickUpDate { get; set; } = DateTime.Now;
    public string? Status { get; set; }

    [Required]
    public string? AssignedDispatcherId { get; set; }
    public string? AssignedDispatcherName { get; set; }

    [Required]
    public string? AssignedTruckId { get; set; }
    public string? AssignedTruckDriverName { get; set; }
}
