using System.ComponentModel;
using Logistics.Application.Abstractions.Agents;
using Logistics.Application.Modules.Operations.Loads.Queries;
using Logistics.Domain.Primitives.Enums;
using Logistics.Shared.Identity.Policies;
using MediatR;

namespace Logistics.Infrastructure.AI.Tools.Operations;

internal sealed class SearchLoadsTool(IMediator mediator)
    : AgentTool<SearchLoadsTool.Input>, IAgentToolMetadata
{
    internal sealed record Input
    {
        [Description("Free-text search over load name and reference fields")]
        public string? Search { get; init; }

        [Description("Filter by load status")]
        public LoadStatus[]? Statuses { get; init; }

        [Description("Filter by load type")]
        public LoadType[]? Types { get; init; }

        [Description("Filter by customer ID (GUID) - find it with search_customers")]
        [AgentEntityId(AgentEntityKind.Customer)]
        public Guid? CustomerId { get; init; }

        [Description("Filter by assigned truck ID (GUID)")]
        [AgentEntityId(AgentEntityKind.Truck)]
        public Guid? TruckId { get; init; }

        [Description("Loads created on or after this date (ISO 8601)")]
        public DateTime? StartDate { get; init; }

        [Description("Loads created on or before this date (ISO 8601)")]
        public DateTime? EndDate { get; init; }

        [Description("Page number when a previous call returned truncated: true")]
        public int? Page { get; init; }
    }

    public static AgentToolDefinition Definition => new(
        "search_loads",
        "Search loads by status, type, customer, truck, date range, or free text. Returns up to 20 loads per page with number, status, origin/destination, delivery cost, and customer. Use for load history questions ('delivered loads last week', 'loads for customer X').")
    {
        RequiredFeature = TenantFeature.Loads,
        RequiredPermission = Permission.Load.View,
        Surfaces = AgentSurfaces.Copilot | AgentSurfaces.Mcp
    };


    protected override async Task<string> ExecuteAsync(Input input, CancellationToken ct)
    {
        var query = new GetLoadsQuery
        {
            Search = input.Search,
            Statuses = input.Statuses,
            Types = input.Types,
            CustomerId = input.CustomerId,
            TruckId = input.TruckId,
            StartDate = input.StartDate,
            EndDate = input.EndDate,
            OrderBy = "-CreatedAt",
            Page = input.Page ?? 1,
            PageSize = ToolResult.MaxResults
        };

        var result = await mediator.Send(query, ct);

        if (!result.IsSuccess)
            return ToolResult.Error(result.Error);

        var items = result.Value?.ToList() ?? [];
        var loads = items.Select(l => new
        {
            id = l.Id,
            number = l.Number,
            name = l.Name,
            status = l.Status.ToString(),
            type = l.Type.ToString(),
            origin = l.OriginAddress.ToString(),
            destination = l.DestinationAddress.ToString(),
            delivery_cost = l.DeliveryCost,
            customer = l.Customer?.Name,
            customer_id = l.Customer?.Id,
            truck_number = l.AssignedTruckNumber,
            delivered_at = l.DeliveredAt?.ToString("yyyy-MM-dd"),
            created_at = l.CreatedAt.ToString("yyyy-MM-dd")
        }).ToList();

        return ToolResult.Paged("loads", loads, result.TotalItems);
    }
}
