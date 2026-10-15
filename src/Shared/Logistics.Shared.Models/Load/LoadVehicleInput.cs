namespace Logistics.Shared.Models;

/// <summary>
/// A vehicle sent with a load create / update. <see cref="Id"/> is null for a new vehicle and the
/// existing id for one being edited, so condition reports stay attached to it.
/// </summary>
public class LoadVehicleInput
{
    public Guid? Id { get; set; }
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
