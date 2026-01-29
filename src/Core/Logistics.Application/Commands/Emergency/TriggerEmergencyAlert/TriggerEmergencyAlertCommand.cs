using Logistics.Application.Abstractions;
using Logistics.Domain.Primitives.Enums.Safety;
using Logistics.Shared.Models;

namespace Logistics.Application.Commands;

public class TriggerEmergencyAlertCommand : IAppRequest<Result<EmergencyAlertDto>>
{
    public required Guid DriverId { get; set; }
    public Guid? TruckId { get; set; }
    public Guid? TripId { get; set; }
    public required EmergencyAlertType Type { get; set; }
    public required EmergencyAlertSource Source { get; set; }
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
    public string? Address { get; set; }
    public string? Description { get; set; }
}
