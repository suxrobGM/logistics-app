using Logistics.Application.Common.Pipeline;
using Logistics.Domain.Primitives.Enums;
using Logistics.Shared.Models;

namespace Logistics.Application.Modules.Compliance.Dvir.Commands;

[RequiresFeature(TenantFeature.Dvir)]
public record CreateDvirReportCommand : ICommand<Result<DvirReportDto>>
{
    public required Guid TruckId { get; set; }
    public required Guid DriverId { get; set; }
    public required DvirType Type { get; set; }
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
    public long? OdometerMeters { get; set; }
    public string? DriverNotes { get; set; }
    public string? DriverSignature { get; set; }
    public Guid? TripId { get; set; }
    public List<CreateDvirDefectRequest> Defects { get; set; } = [];
}
