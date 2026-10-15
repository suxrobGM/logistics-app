using Logistics.Domain.Primitives.Enums;
using Logistics.Domain.Primitives.ValueObjects;

namespace Logistics.Shared.Models;

public record FactoringBatchItemDto
{
    public Guid Id { get; set; }
    public Guid InvoiceId { get; set; }
    public long InvoiceNumber { get; set; }
    public long LoadNumber { get; set; }
    public string? CustomerName { get; set; }
    public Money Amount { get; set; } = null!;
    public FactoringStatus Status { get; set; }
    public decimal? AdvanceAmount { get; set; }
    public decimal? FeeAmount { get; set; }
    public decimal? ReserveAmount { get; set; }
    public decimal ReserveReleasedAmount { get; set; }
    public decimal ReserveOutstanding { get; set; }
    public DateTime? FundedAt { get; set; }
    public DateTime? ReserveReleasedAt { get; set; }
    public DateTime? ChargedBackAt { get; set; }
    public string? RejectionReason { get; set; }

    /// <summary>
    /// Advance the factor's rates give for this invoice; the funding form's starting value.
    /// </summary>
    public decimal EstimatedAdvance { get; set; }

    /// <summary>
    /// Fee the factor's rates give for this invoice; the funding form's starting value.
    /// </summary>
    public decimal EstimatedFee { get; set; }
}
