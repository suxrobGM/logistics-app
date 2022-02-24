namespace Logistics.Application.Contracts.Models;

public class TruckDto
{
    [Required]
    public int? TruckNumber { get; set; }
    public string? DriverId { get; set; }
    public IList<string> CargoesIds { get; set; } = new List<string>();
}
