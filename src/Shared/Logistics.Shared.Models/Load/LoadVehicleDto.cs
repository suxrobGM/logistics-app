namespace Logistics.Shared.Models;

/// <summary>
/// A vehicle carried by a car hauler load.
/// </summary>
public class LoadVehicleDto
{
    public Guid Id { get; set; }
    public string? Vin { get; set; }
    public int? Year { get; set; }
    public string? Make { get; set; }
    public string? Model { get; set; }
    public string? Color { get; set; }
    public string? LotNumber { get; set; }
    public bool IsOperable { get; set; } = true;
    public string? Notes { get; set; }
    public decimal? Rate { get; set; }
}
