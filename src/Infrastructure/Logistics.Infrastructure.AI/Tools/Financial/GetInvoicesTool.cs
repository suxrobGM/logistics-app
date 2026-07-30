using System.Text.Json.Nodes;
using MediatR;
using Logistics.Application.Modules.Financial.Invoices.Queries;
using Logistics.Domain.Primitives.Enums;

namespace Logistics.Infrastructure.AI.Tools.Financial;

internal sealed class GetInvoicesTool(IMediator mediator) : IAIDispatchTool
{
    public string Name => "get_invoices";

    public async Task<string> ExecuteAsync(JsonNode input, CancellationToken ct)
    {
        var query = new GetInvoicesQuery
        {
            LoadId = input.GetGuid("load_id"),
            CustomerId = input.GetGuid("customer_id"),
            Status = input.GetEnum<InvoiceStatus>("status"),
            OverdueOnly = input.GetBool("overdue_only"),
            StartDate = input.GetDate("start_date"),
            EndDate = input.GetDate("end_date"),
            InvoiceType = InvoiceType.Load,
            OrderBy = "-CreatedDate",
            Page = input.GetInt("page") ?? 1,
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
