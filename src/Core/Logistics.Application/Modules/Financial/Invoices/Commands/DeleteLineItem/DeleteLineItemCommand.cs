using Logistics.Shared.Models;

namespace Logistics.Application.Modules.Financial.Invoices.Commands;

/// <summary>
/// Removes a line item from an invoice.
/// </summary>
public record DeleteLineItemCommand(Guid InvoiceId, Guid LineItemId) : ICommand<Result>;
