using System.ComponentModel;
using Logistics.Application.Abstractions.AI;
using Logistics.Application.Modules.Reporting.Reports.Queries;
using Logistics.Domain.Persistence;
using Logistics.Domain.Primitives.Enums;
using Logistics.Mediator;
using Logistics.Shared.Identity.Policies;

namespace Logistics.Infrastructure.AI.Tools.Financial;

internal sealed class GetProfitabilityTool(IMediator mediator, ITenantUnitOfWork tenantUow)
    : AgentTool<GetProfitabilityTool.Input>, IAgentToolMetadata
{
    private const int DefaultDays = 30;

    internal enum Grouping
    {
        Truck,
        Lane,
        Load
    }

    internal sealed record Input
    {
        [Description("What to group delivered loads by")]
        public required Grouping GroupBy { get; init; }

        [Description("Start of the period (ISO 8601). Defaults to 30 days ago.")]
        public DateTime? StartDate { get; init; }

        [Description("End of the period, exclusive (ISO 8601). Defaults to now.")]
        public DateTime? EndDate { get; init; }
    }

    public static AgentToolDefinition Definition => new(
        "get_profitability",
        "Margin of delivered loads for a period, grouped by truck, lane (origin state to destination state) or load, worst margin first, plus company totals. Margin is revenue minus driver pay, the load's share of truck expenses and the factoring fee; net_profit also takes off non-driving staff pay and company overhead. Answers questions like 'which lanes lost money last month'.")
    {
        RequiredFeature = TenantFeature.Reports,
        RequiredPermission = Permission.Stat.View,
        Surfaces = AgentSurfaces.Copilot | AgentSurfaces.Mcp
    };

    protected override async Task<string> ExecuteAsync(Input input, CancellationToken ct)
    {
        var end = input.EndDate ?? DateTime.UtcNow;
        var start = input.StartDate ?? end.AddDays(-DefaultDays);
        var suffix = tenantUow.GetCurrentTenant().Settings.DistanceUnit.Abbreviation();

        var summary = await mediator.Send(new ProfitabilitySummaryQuery { StartDate = start, EndDate = end }, ct);
        if (!summary.IsSuccess || summary.Value is null)
            return ToolResult.Error(summary.Error ?? "No profitability data");

        var rows = await GetRowsAsync(input.GroupBy, start, end, suffix, ct);
        if (rows.Error is not null)
            return ToolResult.Error(rows.Error);

        var s = summary.Value;
        return ToolResult.Ok(new
        {
            totals = new
            {
                loads = s.LoadCount,
                revenue = s.Revenue,
                driver_pay = s.DriverPay,
                truck_costs = s.TruckCosts,
                factoring_fees = s.FactoringFees,
                load_margin = s.LoadMargin,
                unallocated_truck_costs = s.UnallocatedTruckCosts,
                unallocated_driver_pay = s.UnallocatedDriverPay,
                staff_pay = s.StaffPay,
                overhead = s.Overhead,
                net_profit = s.NetProfit,
                unassigned_fuel = s.UnassignedFuel
            },
            rows = rows.Items,
            total_rows = rows.Total,
            truncated = rows.Total > rows.Items.Count
        });
    }

    private async Task<Rows> GetRowsAsync(Grouping groupBy, DateTime start, DateTime end, string suffix, CancellationToken ct)
    {
        switch (groupBy)
        {
            case Grouping.Truck:
            {
                var result = await mediator.Send(new TrucksProfitabilityQuery { StartDate = start, EndDate = end }, ct);
                var trucks = result.Value ?? [];
                return new Rows(result.Error, trucks.Count, trucks.Take(ToolResult.MaxResults).Select(t => (object)new Dictionary<string, object?>
                {
                    ["truck_number"] = t.TruckNumber,
                    ["loads"] = t.LoadCount,
                    ["revenue"] = t.Revenue,
                    ["driver_pay"] = t.DriverPay,
                    ["truck_costs"] = t.TruckCostsTotal,
                    ["margin"] = t.Margin,
                    ["deadhead_percent"] = t.DeadheadPercent,
                    [$"cost_per_{suffix}"] = t.CostPerDistance
                }).ToList());
            }
            case Grouping.Lane:
            {
                var result = await mediator.Send(new LanesProfitabilityQuery { StartDate = start, EndDate = end }, ct);
                var lanes = result.Value ?? [];
                return new Rows(result.Error, lanes.Count, lanes.Take(ToolResult.MaxResults).Select(l => (object)new Dictionary<string, object?>
                {
                    ["lane"] = l.Lane,
                    ["loads"] = l.LoadCount,
                    ["revenue"] = l.Revenue,
                    ["margin"] = l.Margin,
                    [$"revenue_per_{suffix}"] = l.RevenuePerDistance,
                    [$"margin_per_{suffix}"] = l.MarginPerDistance,
                    ["best_customer"] = l.BestCustomer,
                    ["worst_customer"] = l.WorstCustomer
                }).ToList());
            }
            default:
            {
                var result = await mediator.Send(new LoadsProfitabilityQuery
                {
                    StartDate = start,
                    EndDate = end,
                    PageSize = ToolResult.MaxResults
                }, ct);
                var loads = result.Value?.ToList() ?? [];
                return new Rows(result.Error, result.TotalItems, loads.Select(l => (object)new Dictionary<string, object?>
                {
                    ["load_number"] = l.LoadNumber,
                    ["truck_number"] = l.TruckNumber,
                    ["customer"] = l.CustomerName,
                    ["lane"] = l.Lane,
                    ["revenue"] = l.Revenue,
                    ["driver_pay"] = l.DriverPay,
                    ["truck_costs"] = l.TruckCostsTotal,
                    ["factoring_fee"] = l.FactoringFee,
                    ["margin"] = l.Margin,
                    [$"margin_per_{suffix}"] = l.MarginPerDistance
                }).ToList());
            }
        }
    }

    private sealed record Rows(string? Error, int Total, List<object> Items);
}
