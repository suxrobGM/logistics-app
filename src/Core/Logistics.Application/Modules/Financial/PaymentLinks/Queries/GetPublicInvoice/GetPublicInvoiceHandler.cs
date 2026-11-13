using Logistics.Application.Abstractions.BackgroundJobs;
using Logistics.Application.Abstractions;
using Logistics.Application.Modules.Financial.Invoices;
using Logistics.Application.Modules.Financial.PaymentLinks.Commands;
using Logistics.Application.Modules.Financial.PaymentLinks.Services;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Mediator;
using Logistics.Shared.Models;
using Microsoft.Extensions.Logging;

namespace Logistics.Application.Modules.Financial.PaymentLinks.Queries;

internal sealed class GetPublicInvoiceHandler(
    IMasterUnitOfWork masterUow,
    ITenantUnitOfWork tenantUow,
    IPaymentLinkService paymentLinkService,
    ICommandEnqueuer commandEnqueuer,
    ILogger<GetPublicInvoiceHandler> logger)
    : IRequestHandler<GetPublicInvoiceQuery, Result<PublicInvoiceDto>>
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

        var found = await paymentLinkService.FindValidAsync(req.Token, ct);
        if (!found.IsSuccess)
        {
            return Result<PublicInvoiceDto>.Fail(found);
        }

        var paymentLink = found.Value!;

        // Get the invoice
        var invoice = await tenantUow.Repository<Invoice>().GetByIdAsync(paymentLink.InvoiceId, ct);
        if (invoice is null)
        {
            return Result<PublicInvoiceDto>.Fail("Invoice not found.", ErrorCodes.NotFound);
        }

        // Defer access bookkeeping so the read path stays write-free.
        commandEnqueuer.Enqueue(new RecordPaymentLinkAccessCommand
        {
            TenantId = tenant.Id,
            PaymentLinkId = paymentLink.Id
        });

        // Calculate amount due
        var amountDue = invoice.AmountDue;

        // Build the DTO
        var dto = new PublicInvoiceDto
        {
            Id = invoice.Id,
            Number = invoice.Number,
            Status = invoice.Status,
            Subtotal = invoice.Subtotal,
            TaxTotal = invoice.TaxTotal,
            Total = invoice.Total,
            TaxBehavior = invoice.TaxBehavior,
            TaxBreakdown = invoice.GetTaxBreakdown().ToDto(),
            DueDate = invoice.DueDate,
            AmountDue = amountDue,
            LineItems = invoice.LineItems.Select(i => i.ToDto()),
            CompanyName = tenant.DisplayName
        };

        // Add load-specific details if it's a LoadInvoice
        if (invoice is LoadInvoice loadInvoice)
        {
            dto.CustomerName = loadInvoice.Customer?.Name;
            dto.LoadNumber = loadInvoice.Load?.Number;
            if (loadInvoice.AssignedFactor is { } factor)
            {
                dto.RemitTo = new RemitToDto { Name = factor.Name, Address = factor.RemitToAddress };
            }
        }

        logger.LogInformation(
            "Public invoice access for invoice {InvoiceId} via link {LinkId}",
            invoice.Id, paymentLink.Id);

        return Result<PublicInvoiceDto>.Ok(dto);
    }
}
