using System.ComponentModel;
using Logistics.Application.Abstractions.AI;
using Logistics.Application.Modules.Reporting.Reports.Queries;
using Logistics.Domain.Persistence;
using Logistics.Domain.Primitives.Enums;
using Logistics.Mediator;
using Logistics.Shared.Models;
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

        var result = await mediator.Send(new ProfitabilityReportQuery { StartDate = start, EndDate = end }, ct);
        if (!result.IsSuccess || result.Value is null)
            return ToolResult.Error(result.Error ?? "No profitability data");

        var report = result.Value;
        var s = report.Summary;
        var rows = GetRows(input.GroupBy, report, suffix);
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
            rows = rows.Take(ToolResult.MaxResults),
            total_rows = rows.Count,
            truncated = rows.Count > ToolResult.MaxResults
        });
    }

    private static List<Dictionary<string, object?>> GetRows(Grouping groupBy, ProfitabilityReportDto report, string suffix) =>
        groupBy switch
        {
            Grouping.Truck => report.Trucks.Select(t => new Dictionary<string, object?>
            {
                ["truck_number"] = t.TruckNumber,
                ["loads"] = t.LoadCount,
                ["revenue"] = t.Revenue,
                ["driver_pay"] = t.DriverPay,
                ["truck_costs"] = t.TruckCostsTotal,
                ["margin"] = t.Margin,
                ["deadhead_percent"] = t.DeadheadPercent,
                [$"cost_per_{suffix}"] = t.CostPerDistance
            }).ToList(),
            Grouping.Lane => report.Lanes.Select(l => new Dictionary<string, object?>
            {
                ["lane"] = l.Lane,
                ["loads"] = l.LoadCount,
                ["revenue"] = l.Revenue,
                ["margin"] = l.Margin,
                [$"revenue_per_{suffix}"] = l.RevenuePerDistance,
                [$"margin_per_{suffix}"] = l.MarginPerDistance,
                ["best_customer"] = l.BestCustomer,
                ["worst_customer"] = l.WorstCustomer
            }).ToList(),
            _ => report.Loads.Select(l => new Dictionary<string, object?>
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
            }).ToList()
        };
}
