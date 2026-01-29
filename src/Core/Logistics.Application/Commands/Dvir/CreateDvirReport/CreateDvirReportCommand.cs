using Logistics.Application.Abstractions;
using Logistics.Domain.Primitives.Enums.Safety;
using Logistics.Shared.Models;

namespace Logistics.Application.Commands;

public record CreateDvirReportCommand : IAppRequest<Result<DvirReportDto>>
{
    public required Guid TruckId { get; set; }
    public required Guid DriverId { get; set; }
    public required DvirType Type { get; set; }
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
    public int? OdometerReading { get; set; }
    public string? DriverNotes { get; set; }
    public string? DriverSignature { get; set; }
    public Guid? TripId { get; set; }
    public List<CreateDvirDefectRequest> Defects { get; set; } = [];
}
