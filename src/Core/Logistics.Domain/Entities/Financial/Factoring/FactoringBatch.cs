using Logistics.Domain.Core;
using Logistics.Domain.Primitives.Enums;
using Logistics.Domain.Primitives.ValueObjects;

namespace Logistics.Domain.Entities;

/// <summary>A schedule of accounts sent to a factoring company in one submission.</summary>
public class FactoringBatch : AuditableEntity, ITenantEntity
{
    public long Number { get; set; }

    public required Guid FactoringCompanyId { get; set; }
    public virtual FactoringCompany FactoringCompany { get; set; } = null!;

    public FactoringBatchStatus Status { get; set; } = FactoringBatchStatus.Submitted;

    /// <summary>
    /// Sum of the invoice totals on the schedule.
    /// </summary>
    public required decimal Total { get; set; }

    public DateTime SubmittedAt { get; set; }

    /// <summary>
    /// Address the schedule was emailed to.
    /// </summary>
    public required string SubmittedToEmail { get; set; }

    public string? Notes { get; set; }

    public virtual List<FactoringBatchItem> Items { get; set; } = [];

    /// <summary>Why the invoice cannot be submitted to this factoring company, or null when it can. POD and BOL documents are not checked here; the caller checks them.</summary>
    public static string? IneligibilityReason(LoadInvoice invoice, Guid factoringCompanyId)
    {
        if (invoice.FactoringStatus is FactoringStatus.Submitted or FactoringStatus.Funded)
        {
            return "is already submitted to a factor";
        }

        if (invoice.FactoringStatus == FactoringStatus.ChargedBack)
        {
            return "was charged back by the factor";
        }

        if (invoice.Status is not (InvoiceStatus.Issued or InvoiceStatus.Sent or InvoiceStatus.Overdue))
        {
            return $"is {invoice.Status.GetDescription().ToLowerInvariant()}; only issued, unpaid invoices can be factored";
        }

        if (invoice.Payments.Count > 0)
        {
            return "already has payments recorded";
        }

        if (invoice.Customer.FactoringCompanyId is null)
        {
            return $"is billed to {invoice.Customer.Name}, who is not marked as factored";
        }

        return invoice.Customer.FactoringCompanyId != factoringCompanyId
            ? $"is billed to {invoice.Customer.Name}, who is factored with a different company"
            : null;
    }

    /// <summary>Creates a submitted batch for the invoices. The caller adds the returned batch with the repository, which saves its items too.</summary>
    public static FactoringBatch Submit(
        FactoringCompany company, IReadOnlyList<LoadInvoice> invoices, DateTime submittedAt, string? notes)
    {
        if (invoices.Count == 0)
        {
            throw new InvalidOperationException("Select at least one invoice to submit.");
        }

        var batch = new FactoringBatch
        {
            FactoringCompanyId = company.Id,
            FactoringCompany = company,
            Total = invoices.Sum(i => i.Total),
            SubmittedAt = submittedAt,
            SubmittedToEmail = company.SubmissionEmail,
            Notes = notes
        };

        foreach (var invoice in invoices)
        {
            invoice.FactoringStatus = FactoringStatus.Submitted;
            batch.Items.Add(new FactoringBatchItem
            {
                BatchId = batch.Id,
                Batch = batch,
                InvoiceId = invoice.Id,
                Invoice = invoice,
                Amount = invoice.Total
            });
        }

        return batch;
    }

    public void RefreshStatus()
    {
        var funded = Items.Count(i => i.Status is FactoringStatus.Funded or FactoringStatus.ChargedBack);
        var pending = Items.Count(i => i.Status == FactoringStatus.Submitted);

        if (funded == Items.Count)
        {
            Status = FactoringBatchStatus.Funded;
        }
        else if (funded > 0)
        {
            Status = FactoringBatchStatus.PartiallyFunded;
        }
        else
        {
            Status = pending > 0 ? FactoringBatchStatus.Submitted : FactoringBatchStatus.Rejected;
        }
    }
}
