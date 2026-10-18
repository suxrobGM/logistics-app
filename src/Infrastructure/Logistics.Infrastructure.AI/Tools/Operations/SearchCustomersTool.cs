using System.ComponentModel;
using Logistics.Application.Abstractions.Agents;
using Logistics.Application.Modules.IdentityAccess.Customers.Queries;
using Logistics.Domain.Primitives.Enums;
using Logistics.Shared.Identity.Policies;
using Logistics.Mediator;

namespace Logistics.Infrastructure.AI.Tools.Operations;

internal sealed class SearchCustomersTool(IMediator mediator)
    : AgentTool<SearchCustomersTool.Input>, IAgentToolMetadata
{
    internal sealed record Input
    {
        [Description("Customer name or a fragment of it (case-insensitive)")]
        public string? Search { get; init; }
    }

    public static AgentToolDefinition Definition => new(
        "search_customers",
        "Search customers by name (case-insensitive substring). If nothing matches, retry once with a shorter fragment before concluding the customer does not exist. Returns up to 20 customers with ID, name, email, and phone.")
    {
        RequiredFeature = TenantFeature.Customers,
        RequiredPermission = Permission.Customer.View,
        Surfaces = AgentSurfaces.Copilot | AgentSurfaces.Mcp
    };


    protected override async Task<string> ExecuteAsync(Input input, CancellationToken ct)
    {
        var query = new GetCustomersQuery
        {
            Search = input.Search,
            Page = 1,
            PageSize = ToolResult.MaxResults
        };

        var result = await mediator.Send(query, ct);

        if (!result.IsSuccess)
            return ToolResult.Error(result.Error ?? "Unknown error");

        var items = result.Value?.ToList() ?? [];
        var customers = items.Select(c => new
        {
            id = c.Id,
            name = c.Name,
            email = c.Email,
            phone = c.Phone,
            status = c.Status.ToString()
        }).ToList();

        return ToolResult.Paged("customers", customers, result.TotalItems);
    }
}
