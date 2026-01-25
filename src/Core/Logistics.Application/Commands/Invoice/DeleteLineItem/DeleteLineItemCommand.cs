using Logistics.Application.Abstractions;
using Logistics.Shared.Models;

namespace Logistics.Application.Commands;

/// <summary>
/// Removes a line item from an invoice.
/// </summary>
public record DeleteLineItemCommand(Guid InvoiceId, Guid LineItemId) : IAppRequest<Result>;
