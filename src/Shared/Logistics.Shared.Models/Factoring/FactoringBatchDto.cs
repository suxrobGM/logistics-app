using Logistics.Domain.Primitives.Enums;
using Logistics.Domain.Primitives.ValueObjects;

namespace Logistics.Shared.Models;

/// <summary>
/// A schedule of accounts sent to a factoring company. <see cref="Items"/> is only filled on the detail read.
/// </summary>
public record FactoringBatchDto
{
    public Guid Id { get; set; }
    public long Number { get; set; }
    public Guid FactoringCompanyId { get; set; }
    public string? FactoringCompanyName { get; set; }
    public FactoringBatchStatus Status { get; set; }
    public Money Total { get; set; } = null!;
    public DateTime SubmittedAt { get; set; }
    public required string SubmittedToEmail { get; set; }
    public string? Notes { get; set; }
    public int InvoiceCount { get; set; }
    public decimal AdvanceTotal { get; set; }
    public decimal FeeTotal { get; set; }

    /// <summary>
    /// Reserve the factor still holds on the schedule's funded invoices.
    /// </summary>
    public decimal ReserveOutstanding { get; set; }

    public IEnumerable<FactoringBatchItemDto> Items { get; set; } = [];
}
