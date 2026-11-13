using System.ComponentModel;
using Logistics.Application.Abstractions.AI;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Domain.Primitives.Enums;
using Logistics.Shared.Identity.Policies;

namespace Logistics.Infrastructure.AI.Tools.Dispatch;

internal sealed class GetTripTool(ITenantUnitOfWork tenantUow)
    : AgentTool<GetTripTool.Input>, IAgentToolMetadata
{
    internal sealed record Input
    {
        [Description("The trip ID (GUID)")]
        [AgentEntityId(AgentEntityKind.Trip)]
        public required Guid TripId { get; init; }
    }

    public static AgentToolDefinition Definition => new(
        AgentToolNames.GetTrip,
        "Get one trip by ID: status, truck, driver ids (pass them to get_driver_hos_status), and its stops in order with each stop's load, arrival and departure, ETA, appointment window end and whether it is running late. Call this first on a trip alert.")
    {
        RequiredFeature = TenantFeature.Loads,
        RequiredPermission = Permission.Load.View,
        Surfaces = AgentSurfaces.All
    };

    protected override async Task<string> ExecuteAsync(Input input, CancellationToken ct)
    {
        var trip = await tenantUow.Repository<Trip>().GetByIdAsync(input.TripId, ct);
        if (trip is null)
        {
            return ToolResult.Error("Trip not found");
        }

        return ToolResult.Ok(new
        {
            id = trip.Id,
            number = trip.Number,
            name = trip.Name,
            status = trip.Status.ToString(),
            truck_number = trip.Truck?.Number,
            driver_id = trip.DriverId,
            driver_name = await GetNameAsync(trip.DriverId, ct),
            secondary_driver_id = trip.SecondaryDriverId,
            secondary_driver_name = await GetNameAsync(trip.SecondaryDriverId, ct),
            eta_updated_at = Format(trip.EtaUpdatedAt),
            stops = trip.Stops.OrderBy(s => s.Position).Select(s => new
            {
                order = s.Position,
                type = s.Type.ToString(),
                address = s.Address.ToString(),
                load_id = s.LoadId,
                load_number = s.Load.Number,
                arrived_at = Format(s.ArrivedAt),
                departed_at = Format(s.DepartedAt),
                estimated_arrival_at = Format(s.EstimatedArrivalAt),
                appointment_end = Format(s.GetAppointmentEnd()),
                running_late = s.LateNotifiedAt is not null
            })
        });
    }

    private async Task<string?> GetNameAsync(Guid? employeeId, CancellationToken ct)
    {
        return employeeId is { } id && await tenantUow.Repository<Employee>().GetByIdAsync(id, ct) is { } employee
            ? employee.GetFullName()
            : null;
    }

    private static string? Format(DateTime? at)
    {
        return at?.ToString("yyyy-MM-dd HH:mm 'UTC'");
    }
}
