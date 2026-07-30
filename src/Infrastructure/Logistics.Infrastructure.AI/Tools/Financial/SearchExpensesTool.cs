using System.Text.Json.Nodes;
using MediatR;
using Logistics.Application.Modules.Financial.Expenses.Queries;
using Logistics.Domain.Primitives.Enums;

namespace Logistics.Infrastructure.AI.Tools.Financial;

internal sealed class SearchExpensesTool(IMediator mediator) : IAIDispatchTool
{
    public string Name => "search_expenses";

    public async Task<string> ExecuteAsync(JsonNode input, CancellationToken ct)
    {
        var query = new GetExpensesQuery
        {
            Type = input.GetEnum<ExpenseType>("type"),
            Status = input.GetEnum<ExpenseStatus>("status"),
            TruckId = input.GetGuid("truck_id"),
            FromDate = input.GetDate("from_date"),
            ToDate = input.GetDate("to_date"),
            Search = input.GetString("search"),
            Page = input.GetInt("page") ?? 1,
            PageSize = ToolResult.MaxResults
        };

        var result = await mediator.Send(query, ct);

        if (!result.IsSuccess)
            return ToolResult.Error(result.Error);

        var items = result.Value?.ToList() ?? [];
        var expenses = items.Select(e => new
        {
            id = e.Id,
            number = e.Number,
            type = e.Type.ToString(),
            status = e.Status.ToString(),
            amount = e.Amount.Amount,
            currency = e.Amount.Currency,
            vendor = e.VendorName,
            category = e.CompanyCategory?.ToString() ?? e.TruckCategory?.ToString(),
            truck_number = e.Truck?.Number,
            expense_date = e.ExpenseDate.ToString("yyyy-MM-dd"),
            notes = e.Notes
        }).ToList();

        return ToolResult.Paged("expenses", expenses, result.TotalItems);
    }
}
