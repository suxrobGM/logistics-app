using System.ComponentModel.DataAnnotations;

namespace Logistics.Client.Models;

public record CreateTruck
{
    [Required]
    public int? TruckNumber { get; set; }

    [Required]
    public string? DriverId { get; set; }
}
