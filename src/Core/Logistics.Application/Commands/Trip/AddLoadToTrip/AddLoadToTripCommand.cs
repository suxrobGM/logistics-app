using Logistics.Application.Abstractions;
using Logistics.Shared.Models;

namespace Logistics.Application.Commands;

public class AddLoadToTripCommand : IAppRequest
{
    public Guid TripId { get; set; }
    public Guid? ExistingLoadId { get; set; }
    public CreateTripLoadCommand? NewLoad { get; set; } = null!;
}
