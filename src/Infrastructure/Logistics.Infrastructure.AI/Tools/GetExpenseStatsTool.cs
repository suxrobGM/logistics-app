using System.Text.Json;
using System.Text.Json.Nodes;
using MediatR;
using Logistics.Application.Modules.Financial.Expenses.Queries;

namespace Logistics.Infrastructure.AI.Tools;

internal sealed class GetExpenseStatsTool(IMediator mediator) : IAIDispatchTool
{
    public string Name => "get_expense_stats";

    public async Task<string> ExecuteAsync(JsonNode input, CancellationToken ct)
    {
        var query = new GetExpenseStatsQuery
        {
            FromDate = input.GetDate("from_date"),
            ToDate = input.GetDate("to_date")
        };

        var result = await mediator.Send(query, ct);

        if (!result.IsSuccess || result.Value is null)
            return ToolResult.Error(result.Error ?? "No expense data");

        var s = result.Value;
        return JsonSerializer.Serialize(new
        {
            total_amount = s.TotalAmount,
            total_count = s.TotalCount,
            pending_amount = s.PendingAmount,
            approved_amount = s.ApprovedAmount,
            paid_amount = s.PaidAmount,
            by_type = s.ByType.Select(t => new { t.Type, t.Amount, t.Count }),
            by_company_category = s.ByCompanyCategory.Select(c => new { c.Category, c.Amount, c.Count }),
            by_truck_category = s.ByTruckCategory.Select(c => new { c.Category, c.Amount, c.Count }),
            monthly_trend = s.MonthlyTrend.Select(m => new { m.Year, m.Month, m.Amount, m.Count }),
            top_trucks = s.TopTrucks.Select(t => new
            {
                truck_number = t.TruckNumber,
                amount = t.TotalAmount,
                count = t.ExpenseCount
            })
        });
    }
}
