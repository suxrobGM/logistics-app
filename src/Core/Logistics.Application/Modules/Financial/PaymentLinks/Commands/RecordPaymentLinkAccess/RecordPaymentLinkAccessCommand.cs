using Logistics.Application.Abstractions;
using Logistics.Shared.Models;

namespace Logistics.Application.Modules.Financial.PaymentLinks.Commands;

/// <summary>
/// Records a public-payment-link access (counter + timestamp). Enqueued fire-and-forget
/// from the <c>GetPublicInvoice</c> query handler so the read path stays write-free.
/// </summary>
public class RecordPaymentLinkAccessCommand : ICommand
{
    public Guid TenantId { get; set; }
    public Guid PaymentLinkId { get; set; }
}
