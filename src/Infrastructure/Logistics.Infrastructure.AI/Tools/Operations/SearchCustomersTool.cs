using System.Text.Json.Nodes;
using MediatR;
using Logistics.Application.Modules.IdentityAccess.Customers.Queries;

namespace Logistics.Infrastructure.AI.Tools.Operations;

internal sealed class SearchCustomersTool(IMediator mediator) : IAIDispatchTool
{
    public string Name => "search_customers";

    public async Task<string> ExecuteAsync(JsonNode input, CancellationToken ct)
    {
        var query = new GetCustomersQuery
        {
            Search = input.GetString("search"),
            Page = 1,
            PageSize = ToolResult.MaxResults
        };

        var result = await mediator.Send(query, ct);

        if (!result.IsSuccess)
            return ToolResult.Error(result.Error);

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
