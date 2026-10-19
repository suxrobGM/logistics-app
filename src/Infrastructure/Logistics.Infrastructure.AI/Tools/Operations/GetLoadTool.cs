using System.ComponentModel;
using Logistics.Application.Abstractions.Agents;
using Logistics.Application.Modules.Operations.Loads.Queries;
using Logistics.Domain.Persistence;
using Logistics.Domain.Primitives.Enums;
using Logistics.Shared.Identity.Policies;
using Logistics.Shared.Models;
using Logistics.Mediator;

namespace Logistics.Infrastructure.AI.Tools.Operations;

internal sealed class GetLoadTool(IMediator mediator, ITenantUnitOfWork tenantUow)
    : AgentTool<GetLoadTool.Input>, IAgentToolMetadata
{
    internal sealed record Input
    {
        [Description("The load ID (GUID)")]
        [AgentEntityId(AgentEntityKind.Load)]
        public required Guid LoadId { get; init; }
    }

    public static AgentToolDefinition Definition => new(
        "get_load",
        "Get one load by ID: status, type, hazmat class, vehicles on a car hauler load (with any over-capacity warning for the assigned truck), LTL freight details, addresses, distance (in the tenant's unit, named in the key), charges (total_charges is the price: linehaul plus fuel_surcharge and accessorials), customer (with email), assigned truck and trailer, delivery timestamps, and its invoice. ALWAYS call this before create_load_invoice - it shows whether the load is Delivered and already invoiced.")
    {
        RequiredFeature = TenantFeature.Loads,
        RequiredPermission = Permission.Load.View,
        Surfaces = AgentSurfaces.Copilot | AgentSurfaces.Mcp
    };


    protected override async Task<string> ExecuteAsync(Input input, CancellationToken ct)
    {
        var result = await mediator.Send(new GetLoadByIdQuery { Id = input.LoadId }, ct);

        if (!result.IsSuccess || result.Value is null)
            return ToolResult.Error(result.Error ?? "Load not found");

        var l = result.Value;
        var unit = tenantUow.GetCurrentTenant().Settings.DistanceUnit;
        return ToolResult.Ok(new Dictionary<string, object?>
        {
            ["id"] = l.Id,
            ["number"] = l.Number,
            ["name"] = l.Name,
            ["status"] = l.Status.ToString(),
            ["type"] = l.Type.ToString(),
            ["hazmat_class"] = AgentHazmat.DescribeClass(l),
            ["vehicles"] = l.Vehicles.Count > 0 ? AgentVehicleSummary.Describe(l.Vehicles) : null,
            ["vehicle_capacity_warning"] = l.VehicleCapacityWarning,
            ["freight"] = DescribeFreight(l),
            ["origin"] = l.OriginAddress.ToString(),
            ["destination"] = l.DestinationAddress.ToString(),
            [$"distance_{unit.Abbreviation()}"] = DispatchUnits.FromMeters(l.Distance, unit),
            ["linehaul"] = l.DeliveryCost,
            ["fuel_surcharge"] = l.FuelSurcharge != 0 ? l.FuelSurcharge : (decimal?)null,
            ["accessorials"] = l.Accessorials.Count > 0
                ? l.Accessorials.Select(a => new { name = a.Name, total = a.Total }).ToArray()
                : null,
            ["total_charges"] = l.DeliveryCost + l.FuelSurcharge + l.Accessorials.Sum(a => a.Total),
            ["customer"] = l.Customer?.Name,
            ["customer_id"] = l.Customer?.Id,
            ["customer_email"] = l.Customer?.Email,
            ["truck_number"] = l.AssignedTruckNumber,
            ["trailer_number"] = l.TrailerNumber,
            ["dispatched_at"] = l.DispatchedAt?.ToString("yyyy-MM-dd HH:mm"),
            ["delivered_at"] = l.DeliveredAt?.ToString("yyyy-MM-dd HH:mm"),
            ["has_invoice"] = l.Invoice is not null,
            ["invoice_id"] = l.Invoice?.Id,
            ["invoice_status"] = l.Invoice?.Status.ToString(),
            ["invoice_total"] = l.Invoice?.Total.Amount
        });
    }

    /// <summary>"class 70, 6 pallets, weight 4200, not stackable" (weight in the tenant's unit) - null for a load with no LTL details.</summary>
    private static string? DescribeFreight(LoadDto load)
    {
        var parts = new List<string>();
        if (load.FreightClass is { } freightClass)
            parts.Add($"class {freightClass.GetDescription()}");
        if (load.PalletCount is { } pallets)
            parts.Add($"{pallets} pallets");
        if (load.PieceCount is { } pieces)
            parts.Add($"{pieces} pieces");
        if (load.Weight is { } weight)
            parts.Add($"weight {weight:0.#}");
        if (!string.IsNullOrWhiteSpace(load.Dimensions))
            parts.Add(load.Dimensions);

        if (parts.Count == 0)
            return null;

        if (!load.IsStackable)
            parts.Add("not stackable");

        return string.Join(", ", parts);
    }
}
