using Logistics.Application.Abstractions;
using Logistics.Shared.Models;

namespace Logistics.Application.Modules.Financial.Invoices.Commands;

/// <summary>
/// Sends an invoice to a customer via email with a payment link.
/// </summary>
public record SendInvoiceCommand : ICommand<Result>
{
    public required Guid InvoiceId { get; set; }
    public required string RecipientEmail { get; set; }
    public string? PersonalMessage { get; set; }
}
