namespace Logistics.Application.Contracts.Models;

public class TruckDto
{
    public string? Id { get; set; }

    [Required]
    public int? TruckNumber { get; set; }

    [Required]
    public string? DriverId { get; set; }
    public string? DriverName { get; set; }
    public IList<string> CargoesIds { get; set; } = new List<string>();
}
