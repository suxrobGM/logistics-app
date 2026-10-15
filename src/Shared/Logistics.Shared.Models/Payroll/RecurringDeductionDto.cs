using Logistics.Domain.Primitives.Enums;

namespace Logistics.Shared.Models;

/// <summary>
/// A deduction taken from every settlement. Also the input shape of the pay profile update: send
/// <see cref="Id"/> to keep a row, leave it null to add one; rows not sent are removed.
/// </summary>
public record RecurringDeductionDto
{
    public Guid? Id { get; set; }
    public PayDeductionType Type { get; set; }
    public required string Description { get; set; }
    public decimal Amount { get; set; }
    public decimal? EscrowTarget { get; set; }
}
