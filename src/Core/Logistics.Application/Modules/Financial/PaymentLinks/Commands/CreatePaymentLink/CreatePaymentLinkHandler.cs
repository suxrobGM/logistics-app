using Logistics.Application.Abstractions;
using Logistics.Application.Modules.Financial.PaymentLinks.Services;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Mappings;
using Logistics.Mediator;
using Logistics.Shared.Models;
using Microsoft.Extensions.Logging;
using Logistics.Application.Abstractions.CurrentUser;

namespace Logistics.Application.Modules.Financial.PaymentLinks.Commands;

internal sealed class CreatePaymentLinkHandler(
    ITenantUnitOfWork tenantUow,
    ICurrentUserService currentUserService,
    IPaymentLinkService paymentLinkService,
    ILogger<CreatePaymentLinkHandler> logger)
    : IRequestHandler<CreatePaymentLinkCommand, Result<PaymentLinkDto>>
{
    public async Task<Result<PaymentLinkDto>> Handle(CreatePaymentLinkCommand req, CancellationToken ct)
    {
        var currentUserId = currentUserService.GetUserId();

        var invoice = await tenantUow.Repository<Invoice>().GetByIdAsync(req.InvoiceId, ct);
        if (invoice is null)
        {
            return Result<PaymentLinkDto>.Fail("Invoice not found.", ErrorCodes.NotFound);
        }

        // The pay page refuses factored invoices, so a link would be dead on arrival.
        if (invoice is LoadInvoice { AssignedFactor: { } factor })
        {
            return Result<PaymentLinkDto>.Fail(
                $"This invoice is assigned to {factor.Name}; the customer pays {factor.Name} directly, so it cannot have a payment link.");
        }

        var paymentLink = await paymentLinkService.CreateAsync(invoice.Id, req.ExpirationDays, currentUserId, ct);
        await tenantUow.SaveChangesAsync(ct);

        logger.LogInformation(
            "Created payment link {LinkId} for invoice {InvoiceId}, expires at {ExpiresAt}",
            paymentLink.Id, invoice.Id, paymentLink.ExpiresAt);

        return Result<PaymentLinkDto>.Ok(paymentLink.ToDto(paymentLinkService.GetUrl(paymentLink)));
    }
}
