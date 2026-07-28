using System.Text.Json;
using System.Text.Json.Nodes;
using MediatR;
using Logistics.Application.Modules.Financial.Invoices.Queries;
using Logistics.Domain.Primitives.Enums;

namespace Logistics.Infrastructure.AI.Tools;

internal sealed class GetInvoicesTool(IMediator mediator) : IAIDispatchTool
{
    private const int MaxResults = 20;

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
            PageSize = MaxResults
        };

        var result = await mediator.Send(query, ct);

        if (!result.IsSuccess)
            return JsonSerializer.Serialize(new { error = result.Error });

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
        });

        return JsonSerializer.Serialize(new
        {
            invoices,
            count = items.Count,
            total = result.TotalItems,
            truncated = result.TotalItems > items.Count
        });
    }
}
