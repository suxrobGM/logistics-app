using Logistics.Domain.Entities;
using Logistics.Shared.Models;

namespace Logistics.Application.Modules.Operations.Trucks.Services;

public interface ITruckDriverFinder : IApplicationService
{
    /// <summary>
    ///     Loads the driver for a truck's main or secondary seat, refusing one who is not Active or who already
    ///     holds either seat on another truck. <paramref name="truckId" /> is the truck being edited, if any.
    /// </summary>
    Task<Result<Employee>> FindAssignableDriverAsync(Guid driverId, Guid? truckId, CancellationToken ct);
}
