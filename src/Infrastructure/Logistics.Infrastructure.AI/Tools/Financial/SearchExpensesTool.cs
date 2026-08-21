using System.ComponentModel;
using Logistics.Application.Abstractions.Agents;
using Logistics.Application.Modules.Financial.Expenses.Queries;
using Logistics.Domain.Primitives.Enums;
using Logistics.Shared.Identity.Policies;
using MediatR;

namespace Logistics.Infrastructure.AI.Tools.Financial;

internal sealed class SearchExpensesTool(IMediator mediator)
    : AgentTool<SearchExpensesTool.Input>, IAgentToolMetadata
{
    internal sealed record Input
    {
        [Description("Filter by expense type")]
        public ExpenseType? Type { get; init; }

        [Description("Filter by expense status")]
        public ExpenseStatus? Status { get; init; }

        [Description("Filter by truck ID (GUID)")]
        public Guid? TruckId { get; init; }

        [Description("Expenses on or after this date (ISO 8601)")]
        public DateTime? FromDate { get; init; }

        [Description("Expenses on or before this date (ISO 8601)")]
        public DateTime? ToDate { get; init; }

        [Description("Search vendor name or notes")]
        public string? Search { get; init; }

        [Description("Page number when a previous call returned truncated: true")]
        public int? Page { get; init; }
    }

    public static AgentToolDefinition Definition => new(
        "search_expenses",
        "List expense line items filtered by type (Company, Truck, BodyShop), status, truck, date range, or vendor/notes text. Returns up to 20 per page. There is no category filter - for spend-by-category questions use get_expense_stats instead.")
    {
        RequiredFeature = TenantFeature.Expenses,
        RequiredPermission = Permission.Expense.View
    };

    protected override async Task<string> ExecuteAsync(Input input, CancellationToken ct)
    {
        var query = new GetExpensesQuery
        {
            Type = input.Type,
            Status = input.Status,
            TruckId = input.TruckId,
            FromDate = input.FromDate,
            ToDate = input.ToDate,
            Search = input.Search,
            Page = input.Page ?? 1,
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
