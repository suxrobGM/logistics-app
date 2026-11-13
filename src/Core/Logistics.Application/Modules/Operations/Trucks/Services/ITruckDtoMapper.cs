using Logistics.Domain.Entities;
using Logistics.Shared.Models;

namespace Logistics.Application.Modules.Operations.Trucks.Services;

public interface ITruckDtoMapper : IApplicationService
{
    /// <summary>Loads the drivers and their roles for the whole batch, since the navigations lazy-load per row.</summary>
    Task<Dictionary<Guid, TruckDto>> MapAsync(IReadOnlyCollection<Truck> trucks, CancellationToken ct);
}
