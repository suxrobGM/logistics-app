using System.Text.Json;
using System.Text.Json.Nodes;
using MediatR;
using Logistics.Application.Modules.IdentityAccess.Customers.Queries;

namespace Logistics.Infrastructure.AI.Tools;

internal sealed class SearchCustomersTool(IMediator mediator) : IAIDispatchTool
{
    private const int MaxResults = 20;

    public string Name => "search_customers";

    public async Task<string> ExecuteAsync(JsonNode input, CancellationToken ct)
    {
        var query = new GetCustomersQuery
        {
            Search = input.GetString("search"),
            Page = 1,
            PageSize = MaxResults
        };

        var result = await mediator.Send(query, ct);

        if (!result.IsSuccess)
            return JsonSerializer.Serialize(new { error = result.Error });

        var items = result.Value?.ToList() ?? [];
        var customers = items.Select(c => new
        {
            id = c.Id,
            name = c.Name,
            email = c.Email,
            phone = c.Phone,
            status = c.Status.ToString()
        });

        return JsonSerializer.Serialize(new
        {
            customers,
            count = items.Count,
            total = result.TotalItems,
            truncated = result.TotalItems > items.Count
        });
    }
}
