using Logistics.Application.Abstractions;
using Logistics.Application.Abstractions.Documents;
using Logistics.Application.Abstractions.Email;
using Logistics.Application.Abstractions.Email.Models;
using Logistics.Application.Abstractions.Storage;
using Logistics.Application.Modules.Financial.Factoring.Services;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Domain.Primitives.Enums;
using Logistics.Mediator;
using Logistics.Shared.Models;
using Microsoft.Extensions.Logging;

namespace Logistics.Application.Modules.Financial.Factoring.Commands;

internal sealed class SubmitInvoicesToFactorHandler(
    ITenantUnitOfWork tenantUow,
    IInvoicePdfService invoicePdfService,
    IFactoringSchedulePdfService schedulePdfService,
    IBlobStorageService blobStorage,
    IThreadedEmailSender emailSender,
    IEmailTemplateService emailTemplateService,
    ILogger<SubmitInvoicesToFactorHandler> logger)
    : IRequestHandler<SubmitInvoicesToFactorCommand, Result<FactoringBatchDto>>
{
    /// <summary>
    /// Resend caps a message at 40 MB after base64 encoding (a third larger than the raw bytes).
    /// </summary>
    public const long MaxAttachmentBytes = 28L * 1024 * 1024;

    /// <summary>Signed delivery paperwork; one of these must be on the load before a factor will buy the invoice.</summary>
    private static readonly DocumentType[] DeliveryProof = [DocumentType.ProofOfDelivery, DocumentType.BillOfLading];

    /// <summary>Everything the factor gets with the invoice: the delivery proof plus the rate confirmation.</summary>
    private static readonly DocumentType[] SubmittedDocuments = [.. DeliveryProof, DocumentType.Contract];

    public async Task<Result<FactoringBatchDto>> Handle(SubmitInvoicesToFactorCommand req, CancellationToken ct)
    {
        var ids = req.InvoiceIds.Distinct().ToList();
        var invoices = await tenantUow.Repository<LoadInvoice>().GetListAsync(i => ids.Contains(i.Id), ct);
        if (invoices.Count != ids.Count)
        {
            return Result<FactoringBatchDto>.Fail("Some of the selected invoices no longer exist. Reload and try again.");
        }

        var companyId = SingleFactorOf(invoices);
        if (companyId is null)
        {
            return Result<FactoringBatchDto>.Fail(
                "The selected invoices are not all billed to customers factored with the same company. " +
                "Submit one schedule per factoring company.");
        }

        var company = await tenantUow.Repository<FactoringCompany>().GetByIdAsync(companyId.Value, ct);
        if (company is null)
        {
            return Result<FactoringBatchDto>.Fail("Factoring company not found.");
        }

        var loadIds = invoices.Select(i => i.LoadId).ToList();
        var documents = await tenantUow.Repository<LoadDocument>().GetListAsync(
            d => loadIds.Contains(d.LoadId) && d.Status == DocumentStatus.Active && SubmittedDocuments.Contains(d.Type), ct);
        var documentsByLoad = documents.ToLookup(d => d.LoadId);

        var problems = invoices
            .OrderBy(i => i.Number)
            .Select(i => (Invoice: i, Reason: FactoringBatch.IneligibilityReason(i, company.Id)
                ?? (documentsByLoad[i.LoadId].Any(d => DeliveryProof.Contains(d.Type))
                    ? null
                    : $"has no POD or BOL uploaded on load #{i.Load.Number}")))
            .Where(p => p.Reason is not null)
            .Select(p => $"Invoice #{p.Invoice.Number} {p.Reason}.")
            .ToList();
        if (problems.Count > 0)
        {
            return Result<FactoringBatchDto>.Fail(string.Join(" ", problems));
        }

        var tenant = tenantUow.GetCurrentTenant();
        var attachments = new List<EmailFileAttachment>();
        foreach (var invoice in invoices.OrderBy(i => i.Number))
        {
            attachments.Add(new EmailFileAttachment(
                $"Invoice-{invoice.Number}.pdf", invoicePdfService.GenerateLoadInvoicePdf(invoice, tenant), "application/pdf"));
            foreach (var document in documentsByLoad[invoice.LoadId])
            {
                attachments.Add(await DownloadAsync(invoice, document, ct));
            }
        }

        if (attachments.Sum(a => (long)a.Content.Length) > MaxAttachmentBytes)
        {
            return Result<FactoringBatchDto>.Fail(
                "The invoices and their paperwork are too large for one email. Submit fewer invoices at a time.");
        }

        var previousStatuses = invoices.ToDictionary(i => i.Id, i => i.FactoringStatus);
        var batch = FactoringBatch.Submit(company, invoices, DateTime.UtcNow, req.Notes);
        await tenantUow.Repository<FactoringBatch>().AddAsync(batch, ct);
        await tenantUow.SaveChangesAsync(ct);

        attachments.Insert(0, new EmailFileAttachment(
            $"Schedule-{batch.Number}.pdf", schedulePdfService.GenerateSchedulePdf(batch, tenant), "application/pdf"));

        if (!await SendAsync(batch, company, invoices, tenant, attachments, ct))
        {
            // Nothing reached the factor, so the schedule must not exist either.
            tenantUow.Repository<FactoringBatch>().Delete(batch);
            foreach (var invoice in invoices)
            {
                invoice.FactoringStatus = previousStatuses[invoice.Id];
            }
            await tenantUow.SaveChangesAsync(ct);
            return Result<FactoringBatchDto>.Fail(
                $"Could not email the schedule to {company.SubmissionEmail}. Nothing was submitted; try again.");
        }

        logger.LogInformation(
            "Submitted schedule {Number} with {Count} invoices to factoring company {FactoringCompanyId}",
            batch.Number, invoices.Count, company.Id);
        return Result<FactoringBatchDto>.Ok(await FactoringBatchDtos.DetailAsync(tenantUow, batch, ct));
    }

    private static Guid? SingleFactorOf(IEnumerable<LoadInvoice> invoices)
    {
        var factors = invoices.Select(i => i.Customer.FactoringCompanyId).Distinct().ToList();
        return factors is [{ } only] ? only : null;
    }

    private async Task<EmailFileAttachment> DownloadAsync(LoadInvoice invoice, LoadDocument document, CancellationToken ct)
    {
        await using var stream = await blobStorage.DownloadAsync(document.BlobContainer, document.BlobPath, ct);
        using var buffer = new MemoryStream();
        await stream.CopyToAsync(buffer, ct);
        var fileName = $"Load-{invoice.Load.Number}-{document.Type}-{document.OriginalFileName}";
        return new EmailFileAttachment(fileName, buffer.ToArray(), document.ContentType);
    }

    private async Task<bool> SendAsync(
        FactoringBatch batch, FactoringCompany company, IReadOnlyList<LoadInvoice> invoices, Tenant tenant,
        IReadOnlyList<EmailFileAttachment> attachments, CancellationToken ct)
    {
        var companyName = tenant.CompanyName ?? tenant.Name;
        var model = new FactoringScheduleEmailModel
        {
            CompanyName = companyName,
            FactorName = company.Name,
            ScheduleNumber = batch.Number,
            Total = $"{batch.Total.Amount:N2} {batch.Total.Currency}",
            Notes = batch.Notes,
            ReplyEmail = tenant.BillingEmail,
            Invoices = invoices.OrderBy(i => i.Number).Select(i => new FactoringScheduleInvoiceEmailModel
            {
                InvoiceNumber = i.Number,
                LoadNumber = i.Load.Number,
                CustomerName = i.Customer.Name,
                Amount = $"{i.Total.Amount:N2} {i.Total.Currency}"
            }).ToList()
        };

        var body = await emailTemplateService.RenderAsync("FactoringSchedule", model);
        var result = await emailSender.SendAsync(new ThreadedEmail(
            To: company.SubmissionEmail,
            Subject: $"Schedule of accounts #{batch.Number} from {companyName}",
            HtmlBody: body,
            ReplyTo: tenant.BillingEmail,
            Attachments: attachments), ct);
        return result.Success;
    }
}
