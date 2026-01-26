using Logistics.Application.Abstractions;
using Logistics.Domain.Primitives.Enums;
using Logistics.Shared.Models;

namespace Logistics.Application.Commands;

/// <summary>
/// Updates an existing line item on an invoice.
/// </summary>
public record UpdateLineItemCommand : IAppRequest<Result<InvoiceLineItemDto>>
{
    public required Guid InvoiceId { get; set; }
    public required Guid LineItemId { get; set; }
    public string? Description { get; set; }
    public InvoiceLineItemType? Type { get; set; }
    public decimal? Amount { get; set; }
    public int? Quantity { get; set; }
    public string? Notes { get; set; }
}
