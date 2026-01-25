using Logistics.Application.Abstractions;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Mappings;
using Logistics.Shared.Models;
using Microsoft.Extensions.Logging;

namespace Logistics.Application.Queries;

internal sealed class GetPublicInvoiceHandler(
    IMasterUnitOfWork masterUow,
    ITenantUnitOfWork tenantUow,
    ILogger<GetPublicInvoiceHandler> logger)
    : IAppRequestHandler<GetPublicInvoiceQuery, Result<PublicInvoiceDto>>
{
    public async Task<Result<PublicInvoiceDto>> Handle(GetPublicInvoiceQuery req, CancellationToken ct)
    {
        // Get the tenant
        var tenant = await masterUow.Repository<Tenant>().GetByIdAsync(req.TenantId, ct);
        if (tenant is null)
        {
            return Result<PublicInvoiceDto>.Fail("Invalid payment link.");
        }

        // Switch to tenant database
        tenantUow.SetCurrentTenant(tenant);

        // Find the payment link by token
        var paymentLink = await tenantUow.Repository<PaymentLink>()
            .GetAsync(p => p.Token == req.Token, ct);

        if (paymentLink is null)
        {
            return Result<PublicInvoiceDto>.Fail("Invalid payment link.");
        }

        if (!paymentLink.IsValid)
        {
            return Result<PublicInvoiceDto>.Fail("This payment link has expired or been revoked.");
        }

        // Get the invoice
        var invoice = await tenantUow.Repository<Invoice>().GetByIdAsync(paymentLink.InvoiceId, ct);
        if (invoice is null)
        {
            return Result<PublicInvoiceDto>.Fail("Invoice not found.");
        }

        // Record the access
        paymentLink.RecordAccess();
        tenantUow.Repository<PaymentLink>().Update(paymentLink);
        await tenantUow.SaveChangesAsync(ct);

        // Calculate amount due
        var totalPaid = invoice.Payments.Sum(p => p.Amount.Amount);
        var amountDue = invoice.Total.Amount - totalPaid;

        // Build the DTO
        var dto = new PublicInvoiceDto
        {
            Id = invoice.Id,
            Number = invoice.Number,
            Status = invoice.Status,
            Total = invoice.Total,
            DueDate = invoice.DueDate,
            AmountDue = amountDue,
            LineItems = invoice.LineItems.Select(i => i.ToDto()),
            CompanyName = tenant.CompanyName ?? tenant.Name
        };

        // Add load-specific details if it's a LoadInvoice
        if (invoice is LoadInvoice loadInvoice)
        {
            dto.CustomerName = loadInvoice.Customer?.Name;
            dto.LoadNumber = loadInvoice.Load?.Number;
        }

        logger.LogInformation(
            "Public invoice access for invoice {InvoiceId} via link {LinkId}",
            invoice.Id, paymentLink.Id);

        return Result<PublicInvoiceDto>.Ok(dto);
    }
}
