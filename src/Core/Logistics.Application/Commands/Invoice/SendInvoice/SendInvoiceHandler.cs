using Logistics.Application.Abstractions;
using Logistics.Application.Contracts.Models.Email;
using Logistics.Application.Contracts.Services.Email;
using Logistics.Application.Services;
using Logistics.Application.Utilities;
using Logistics.Domain.Entities;
using Logistics.Domain.Options;
using Logistics.Domain.Persistence;
using Logistics.Domain.Primitives.Enums;
using Logistics.Shared.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Logistics.Application.Commands;

internal sealed class SendInvoiceHandler(
    ITenantUnitOfWork tenantUow,
    IEmailSender emailSender,
    IEmailTemplateService emailTemplateService,
    ICurrentUserService currentUserService,
    IOptions<CustomerPortalOptions> portalOptions,
    ILogger<SendInvoiceHandler> logger)
    : IAppRequestHandler<SendInvoiceCommand, Result>
{
    private const int DefaultExpirationDays = 30;

    public async Task<Result> Handle(SendInvoiceCommand req, CancellationToken ct)
    {
        var currentUserId = currentUserService.GetUserId();

        var invoice = await tenantUow.Repository<Invoice>().GetByIdAsync(req.InvoiceId, ct);
        if (invoice is null)
        {
            return Result.Fail("Invoice not found.");
        }

        if (invoice.Status == InvoiceStatus.Cancelled)
        {
            return Result.Fail("Cannot send a cancelled invoice.");
        }

        var tenant = tenantUow.GetCurrentTenant();

        // Get or create a payment link for this invoice
        var paymentLink = invoice.PaymentLinks.FirstOrDefault(p => p.IsValid);
        if (paymentLink is null)
        {
            paymentLink = new PaymentLink
            {
                Token = TokenGenerator.GenerateSecureToken(),
                InvoiceId = invoice.Id,
                ExpiresAt = DateTime.UtcNow.AddDays(DefaultExpirationDays),
                IsActive = true,
                CreatedByUserId = currentUserId ?? Guid.Empty
            };
            await tenantUow.Repository<PaymentLink>().AddAsync(paymentLink, ct);
        }

        var paymentUrl = $"{portalOptions.Value.BaseUrl}/pay/{tenant.Id}/{paymentLink.Token}";
        var companyName = tenant.CompanyName ?? tenant.Name;

        // Calculate amount due
        var totalPaid = invoice.Payments.Sum(p => p.Amount.Amount);
        var amountDue = invoice.Total.Amount - totalPaid;

        // Build line items for email
        var lineItems = invoice.LineItems.Select(li => new InvoiceLineItemEmailModel
        {
            Description = li.Description,
            Amount = li.Amount.Amount.ToString("C"),
            Quantity = li.Quantity,
            Total = (li.Amount.Amount * li.Quantity).ToString("C")
        }).ToList();

        // Get additional details based on invoice type
        string? customerName = null;
        long? loadNumber = null;
        if (invoice is LoadInvoice loadInvoice)
        {
            customerName = loadInvoice.Customer?.Name;
            loadNumber = loadInvoice.Load?.Number;
        }

        var model = new InvoiceEmailModel
        {
            CompanyName = companyName,
            InvoiceNumber = invoice.Number,
            Total = invoice.Total.Amount.ToString("C"),
            AmountDue = amountDue.ToString("C"),
            Currency = invoice.Total.Currency,
            DueDate = invoice.DueDate?.ToString("MMMM dd, yyyy"),
            CustomerName = customerName,
            LoadNumber = loadNumber,
            PersonalMessage = req.PersonalMessage,
            PaymentUrl = paymentUrl,
            ExpiresAt = paymentLink.ExpiresAt.ToString("MMMM dd, yyyy"),
            LineItems = lineItems
        };

        var subject = $"Invoice #{invoice.Number} from {companyName}";
        var body = await emailTemplateService.RenderAsync("Invoice", model);

        var sent = await emailSender.SendEmailAsync(req.RecipientEmail, subject, body);
        if (!sent)
        {
            return Result.Fail("Failed to send email. Please try again.");
        }

        // Update invoice sent status
        invoice.SentAt = DateTime.UtcNow;
        invoice.SentToEmail = req.RecipientEmail;
        tenantUow.Repository<Invoice>().Update(invoice);
        await tenantUow.SaveChangesAsync(ct);

        logger.LogInformation(
            "Sent invoice {InvoiceId} to {Email}",
            invoice.Id, req.RecipientEmail);

        return Result.Ok();
    }
}
