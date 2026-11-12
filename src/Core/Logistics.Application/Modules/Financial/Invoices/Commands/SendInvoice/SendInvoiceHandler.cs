using Logistics.Application.Abstractions;
using Logistics.Application.Abstractions.Email.Models;
using Logistics.Application.Abstractions.Email;
using Logistics.Application.Modules.Financial.PaymentLinks.Services;
using Logistics.Domain.Entities;
using Logistics.Domain.Primitives.ValueObjects;
using Logistics.Domain.Persistence;
using Logistics.Domain.Primitives.Enums;
using Logistics.Mediator;
using Logistics.Shared.Models;
using Microsoft.Extensions.Logging;
using Logistics.Application.Abstractions.CurrentUser;

namespace Logistics.Application.Modules.Financial.Invoices.Commands;

internal sealed class SendInvoiceHandler(
    ITenantUnitOfWork tenantUow,
    IEmailSender emailSender,
    IEmailTemplateService emailTemplateService,
    ICurrentUserService currentUserService,
    IPaymentLinkService paymentLinkService,
    ILogger<SendInvoiceHandler> logger)
    : IRequestHandler<SendInvoiceCommand, Result>
{
    private const int DefaultExpirationDays = 30;

    public async Task<Result> Handle(SendInvoiceCommand req, CancellationToken ct)
    {
        var currentUserId = currentUserService.GetUserId();

        var invoice = await tenantUow.Repository<Invoice>().GetByIdAsync(req.InvoiceId, ct);
        if (invoice is null)
        {
            return Result.Fail("Invoice not found.", ErrorCodes.NotFound);
        }

        if (invoice.Status == InvoiceStatus.Cancelled)
        {
            return Result.Fail("Cannot send a cancelled invoice.");
        }

        var tenant = tenantUow.GetCurrentTenant();

        // EU tenants legally cannot send VAT-bearing invoices without their own VAT number.
        if (tenant.Settings?.Region == Region.EU && string.IsNullOrWhiteSpace(tenant.VatNumber))
        {
            return Result.Fail(
                "Tenant VAT number is required to send invoices for EU tenants. " +
                "Set it in Settings > Company before sending.");
        }

        // A factored invoice is paid to the factor, so it gets the notice of assignment instead of a card payment link.
        var factor = (invoice as LoadInvoice)?.AssignedFactor;
        PaymentLink? paymentLink = null;
        if (factor is null)
        {
            paymentLink = invoice.PaymentLinks.FirstOrDefault(p => p.IsValid)
                          ?? await paymentLinkService.CreateAsync(invoice.Id, DefaultExpirationDays, currentUserId, ct);
        }

        var paymentUrl = paymentLink is null ? null : paymentLinkService.GetUrl(paymentLink);
        var companyName = tenant.DisplayName;

        // Calculate amount due
        var amountDue = invoice.AmountDue;

        // Build line items for email
        var lineItems = invoice.LineItems.Select(li => new InvoiceLineItemEmailModel
        {
            Description = li.Description,
            Amount = li.Amount.Format(),
            Quantity = li.Quantity,
            Total = Money.Format(li.Amount.Amount * li.Quantity, li.Amount.Currency)
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
            Total = invoice.Total.Format(),
            AmountDue = Money.Format(amountDue, invoice.Total.Currency),
            Currency = invoice.Total.Currency,
            DueDate = invoice.DueDate?.ToString("MMMM dd, yyyy"),
            CustomerName = customerName,
            LoadNumber = loadNumber,
            PersonalMessage = req.PersonalMessage,
            PaymentUrl = paymentUrl,
            ExpiresAt = paymentLink?.ExpiresAt.ToString("MMMM dd, yyyy"),
            RemitToName = factor?.Name,
            RemitToAddress = factor?.RemitToAddress is { } a
                ? string.Join(", ", new[] { a.Line1, a.Line2, a.City, $"{a.State} {a.ZipCode}".Trim(), a.Country }
                    .Where(s => !string.IsNullOrWhiteSpace(s)))
                : null,
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
        await tenantUow.SaveChangesAsync(ct);

        logger.LogInformation(
            "Sent invoice {InvoiceId} to {Email}",
            invoice.Id, req.RecipientEmail);

        return Result.Ok();
    }
}
