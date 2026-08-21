using System.ComponentModel;
using Logistics.Application.Abstractions.Agents;
using Logistics.Application.Modules.Financial.Invoices.Queries;
using Logistics.Domain.Primitives.Enums;
using Logistics.Shared.Identity.Policies;
using MediatR;

namespace Logistics.Infrastructure.AI.Tools.Financial;

internal sealed class GetInvoicesTool(IMediator mediator)
    : AgentTool<GetInvoicesTool.Input>, IAgentToolMetadata
{
    internal sealed record Input
    {
        [Description("Filter by load ID (GUID)")]
        public Guid? LoadId { get; init; }

        [Description("Filter by customer ID (GUID)")]
        public Guid? CustomerId { get; init; }

        [Description("Filter by invoice status")]
        public InvoiceStatus? Status { get; init; }

        [Description("Only invoices past their due date")]
        public bool? OverdueOnly { get; init; }

        [Description("Invoices created on or after this date (ISO 8601)")]
        public DateTime? StartDate { get; init; }

        [Description("Invoices created on or before this date (ISO 8601)")]
        public DateTime? EndDate { get; init; }

        [Description("Page number when a previous call returned truncated: true")]
        public int? Page { get; init; }
    }

    public static AgentToolDefinition Definition => new(
        "get_invoices",
        "List load invoices filtered by load, customer, status, overdue flag, or date range. Returns up to 20 per page with number, status, total, due date, and customer.")
    {
        RequiredFeature = TenantFeature.Invoices,
        RequiredPermission = Permission.Invoice.View
    };

    protected override async Task<string> ExecuteAsync(Input input, CancellationToken ct)
    {
        var query = new GetInvoicesQuery
        {
            LoadId = input.LoadId,
            CustomerId = input.CustomerId,
            Status = input.Status,
            OverdueOnly = input.OverdueOnly,
            StartDate = input.StartDate,
            EndDate = input.EndDate,
            InvoiceType = InvoiceType.Load,
            // Entity property name, not the DTO's CreatedDate - the sort is applied to the query.
            OrderBy = "-CreatedAt",
            Page = input.Page ?? 1,
            PageSize = ToolResult.MaxResults
        };

        var result = await mediator.Send(query, ct);

        if (!result.IsSuccess)
            return ToolResult.Error(result.Error);

        var items = result.Value?.ToList() ?? [];
        var invoices = items.Select(i => new
        {
            id = i.Id,
            number = i.Number,
            status = i.Status.ToString(),
            total = i.Total.Amount,
            currency = i.Total.Currency,
            due_date = i.DueDate?.ToString("yyyy-MM-dd"),
            sent_at = i.SentAt?.ToString("yyyy-MM-dd"),
            load_number = i.LoadNumber,
            load_id = i.LoadId,
            customer = i.Customer?.Name,
            customer_id = i.CustomerId,
            created_at = i.CreatedDate.ToString("yyyy-MM-dd")
        }).ToList();

        return ToolResult.Paged("invoices", invoices, result.TotalItems);
    }
}
