using Logistics.Domain.Primitives.Enums;

namespace Logistics.Shared.Models;

public record TrailerDto
{
    public Guid Id { get; set; }
    public required string Number { get; set; }
    public TrailerType Type { get; set; }
    public TrailerStatus Status { get; set; }
    public string? Vin { get; set; }
    public string? LicensePlate { get; set; }
    public string? LicensePlateState { get; set; }
    public int? Year { get; set; }
    public string? Make { get; set; }
    public string? Model { get; set; }
    public decimal? LengthMeters { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; }

    /// <summary>
    ///     The truck this trailer is currently hooked to, if any.
    /// </summary>
    public Guid? CurrentTruckId { get; set; }

    public string? CurrentTruckNumber { get; set; }
}
