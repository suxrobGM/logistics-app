using Logistics.Domain.Primitives.Enums;

namespace Logistics.Shared.Models;

/// <summary>
/// Invoice information visible to customers in the portal.
/// </summary>
public class PortalInvoiceDto
{
    public Guid Id { get; set; }
    public long Number { get; set; }
    public InvoiceStatus Status { get; set; }
    public decimal Total { get; set; }

    // Related load info
    public Guid? LoadId { get; set; }
    public long? LoadNumber { get; set; }
    public string? LoadName { get; set; }

    // Timestamps
    public DateTime CreatedAt { get; set; }
    public DateTime? PaidAt { get; set; }
    public DateTime? DueDate { get; set; }
}
