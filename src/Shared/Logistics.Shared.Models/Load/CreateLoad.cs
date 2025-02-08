using System.ComponentModel.DataAnnotations;
using Logistics.Shared.Consts;

namespace Logistics.Shared.Models;

public record CreateLoad
{
    public string? Name { get; set; }

    [Required]
    public string? SourceAddress { get; set; }

    [Required]
    public string? DestinationAddress { get; set; }

    [Required]
    [Range((double)LoadConsts.MinDeliveryCost, (double)LoadConsts.MaxDeliveryCost)]
    public decimal DeliveryCost { get; set; }

    [Required]
    public double Distance { get; set; }

    [Required]
    public string? AssignedDispatcherId { get; set; }

    [Required]
    public string? AssignedDriverId { get; set; }
}
