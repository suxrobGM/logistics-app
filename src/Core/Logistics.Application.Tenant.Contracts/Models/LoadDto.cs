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
    [Range(LoadConsts.MinDeliveryCost, LoadConsts.MaxDeliveryCost)]
    public decimal DeliveryCost { get; set; }

    [Required]
    [Range(LoadConsts.MinDistance, LoadConsts.MaxDistance)]
    public double Distance { get; set; }
    
    public DateTime DispatchedDate { get; set; } = DateTime.Now;
    public DateTime? PickUpDate { get; set; }
    public DateTime? DeliveryDate { get; set; }
    public string? Status { get; set; }

    [Required]
    public string? AssignedDispatcherId { get; set; }
    public string? AssignedDispatcherName { get; set; }
    
    [Required]
    public string? AssignedDriverId { get; set; }
    public string? AssignedDriverName { get; set; }
    public string? AssignedTruckId { get; set; }
}
