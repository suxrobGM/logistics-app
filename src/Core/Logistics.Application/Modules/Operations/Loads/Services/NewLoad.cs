using Logistics.Application.Modules.Operations.Loads.Commands;

namespace Logistics.Application.Modules.Operations.Loads.Services;

/// <summary>A load to create: its details, plus the truck and trip it starts on.</summary>
public record NewLoad(ILoadDetails Details, Guid? TruckId, Guid? TripId = null);
