using System.ComponentModel;
using System.Text.Json;
using Logistics.Application.Abstractions.Agents;
using Logistics.Application.Modules.Financial.Expenses.Queries;
using Logistics.Domain.Primitives.Enums;
using Logistics.Shared.Identity.Policies;
using MediatR;

namespace Logistics.Infrastructure.AI.Tools.Financial;

internal sealed class GetExpenseStatsTool(IMediator mediator)
    : AgentTool<GetExpenseStatsTool.Input>, IAgentToolMetadata
{
    internal sealed record Input
    {
        [Description("Start of the period (ISO 8601)")]
        public DateTime? FromDate { get; init; }

        [Description("End of the period (ISO 8601)")]
        public DateTime? ToDate { get; init; }
    }

    public static AgentToolDefinition Definition => new(
        "get_expense_stats",
        "Expense rollups for a date range: totals by approval status, by type, by company/truck category, 12-month trend, and top trucks by spend. Prefer this over search_expenses for 'how much did we spend on X' questions.")
    {
        RequiredFeature = TenantFeature.Expenses,
        RequiredPermission = Permission.Expense.View
    };

    protected override async Task<string> ExecuteAsync(Input input, CancellationToken ct)
    {
        var query = new GetExpenseStatsQuery
        {
            FromDate = input.FromDate,
            ToDate = input.ToDate
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
