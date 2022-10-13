using Logistics.Sdk.Constants;
using System.ComponentModel.DataAnnotations;

namespace Logistics.Sdk.Models;

public record CreateLoad
{
    public string? Name { get; set; }

    [Required]
    public string? SourceAddress { get; set; }

    [Required]
    public string? DestinationAddress { get; set; }

    [Required]
    [Range(LoadConsts.MinDeliveryCost, LoadConsts.MaxDeliveryCost)]
    public double DeliveryCost { get; set; }

    [Required]
    public double Distance { get; set; }

    [Required]
    public string? AssignedDispatcherId { get; set; }

    [Required]
    public string? AssignedDriverId { get; set; }
}
