using Logistics.Application.Abstractions;
using Logistics.Domain.Primitives.Enums;
using Logistics.Shared.Models;

namespace Logistics.Application.Commands;

/// <summary>
/// Adds a line item to an invoice.
/// </summary>
public record AddLineItemCommand : IAppRequest<Result<InvoiceLineItemDto>>
{
    public required Guid InvoiceId { get; set; }
    public required string Description { get; set; }
    public required InvoiceLineItemType Type { get; set; }
    public required decimal Amount { get; set; }
    public int Quantity { get; set; } = 1;
    public string? Notes { get; set; }
}
