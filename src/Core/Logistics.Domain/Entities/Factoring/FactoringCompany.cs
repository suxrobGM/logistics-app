using Logistics.Domain.Core;
using Logistics.Domain.Primitives.ValueObjects;

namespace Logistics.Domain.Entities;

/// <summary>
/// A factoring company the carrier sells its invoices to. Customers marked as factored point here, their
/// invoices carry its notice of assignment, and schedules of accounts are emailed to it.
/// </summary>
public class FactoringCompany : AuditableEntity, ITenantEntity
{
    public required string Name { get; set; }

    /// <summary>
    /// Where customers of factored invoices must send payment; printed on the notice of assignment.
    /// </summary>
    public Address? RemitToAddress { get; set; }

    /// <summary>
    /// Inbox the factor takes schedules of accounts at.
    /// </summary>
    public required string SubmissionEmail { get; set; }

    public string? Phone { get; set; }

    /// <summary>
    /// Percent of an invoice paid to the carrier when the factor funds it.
    /// </summary>
    public decimal AdvanceRatePercent { get; set; }

    /// <summary>
    /// Percent of an invoice the factor keeps as its fee.
    /// </summary>
    public decimal FeePercent { get; set; }

    /// <summary>
    /// Percent of an invoice held back until the customer pays the factor: whatever the advance and the
    /// fee leave over, so advance + fee + reserve is always the whole invoice.
    /// </summary>
    public decimal ReservePercent => Math.Max(0m, 100m - AdvanceRatePercent - FeePercent);

    /// <summary>
    /// The advance, fee and reserve this factor's rates give for an invoice total, as the default the
    /// funding form starts from before the carrier enters what the factor actually paid.
    /// </summary>
    public FactoringAmounts EstimateAmounts(decimal invoiceTotal)
    {
        var advance = Math.Round(invoiceTotal * AdvanceRatePercent / 100m, 2, MidpointRounding.AwayFromZero);
        var fee = Math.Round(invoiceTotal * FeePercent / 100m, 2, MidpointRounding.AwayFromZero);
        return FactoringAmounts.FromAdvanceAndFee(invoiceTotal, advance, fee);
    }
}
