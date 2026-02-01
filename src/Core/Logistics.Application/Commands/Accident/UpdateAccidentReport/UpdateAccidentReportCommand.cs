using Logistics.Application.Abstractions;
using Logistics.Domain.Primitives.Enums.Safety;
using Logistics.Shared.Models;

namespace Logistics.Application.Commands;

public record UpdateAccidentReportCommand : IAppRequest<Result<AccidentReportDto>>
{
    public required Guid Id { get; set; }
    public required Guid TruckId { get; set; }
    public required Guid DriverId { get; set; }
    public required AccidentType Type { get; set; }
    public required AccidentSeverity Severity { get; set; }
    public required DateTime AccidentDateTime { get; set; }
    public required string Location { get; set; }
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
    public required string Description { get; set; }
    public string? WeatherConditions { get; set; }
    public string? RoadConditions { get; set; }
    public bool InjuriesReported { get; set; }
    public int? NumberOfInjuries { get; set; }
    public string? InjuryDescription { get; set; }
    public bool FatalitiesReported { get; set; }
    public int? NumberOfFatalities { get; set; }
    public bool VehicleTowed { get; set; }
    public string? TowCompany { get; set; }
    public decimal? EstimatedDamage { get; set; }
    public string? DamageDescription { get; set; }
    public bool HazmatInvolved { get; set; }
    public string? HazmatDescription { get; set; }
    public Guid? TripId { get; set; }
}
