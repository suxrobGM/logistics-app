using Logistics.Application.Abstractions;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Domain.Primitives.Enums;
using Logistics.Domain.Primitives.ValueObjects;
using Logistics.Shared.Models;
using Microsoft.Extensions.Logging;
using Logistics.Application.Abstractions.Payments.Stripe;
using Logistics.Application.Abstractions.Payments;

namespace Logistics.Application.Modules.Financial.Payments.Commands;

internal sealed class CreatePublicCheckoutSessionHandler(
    IMasterUnitOfWork masterUow,
    ITenantUnitOfWork tenantUow,
    IStripeConnectService stripeConnectService,
    ILogger<CreatePublicCheckoutSessionHandler> logger)
    : IAppRequestHandler<CreatePublicCheckoutSessionCommand, Result<PublicCheckoutSessionDto>>
{
    public async Task<Result<PublicCheckoutSessionDto>> Handle(
        CreatePublicCheckoutSessionCommand req, CancellationToken ct)
    {
        var tenant = await masterUow.Repository<Tenant>().GetByIdAsync(req.TenantId, ct);
        if (tenant is null)
        {
            return Result<PublicCheckoutSessionDto>.Fail("Invalid payment link.");
        }

        if (string.IsNullOrEmpty(tenant.StripeConnectedAccountId))
        {
            return Result<PublicCheckoutSessionDto>.Fail("This company is not set up to receive payments.");
        }

        if (!tenant.ChargesEnabled)
        {
            return Result<PublicCheckoutSessionDto>.Fail(
                "This company cannot currently accept payments. Please contact them directly.");
        }

        tenantUow.SetCurrentTenant(tenant);

        var paymentLink = await tenantUow.Repository<PaymentLink>()
            .GetAsync(p => p.Token == req.Token, ct);

        if (paymentLink is null || !paymentLink.IsValid)
        {
            return Result<PublicCheckoutSessionDto>.Fail(
                "This payment link has expired or been revoked.");
        }

        var invoice = await tenantUow.Repository<Invoice>().GetByIdAsync(paymentLink.InvoiceId, ct);
        if (invoice is null)
        {
            return Result<PublicCheckoutSessionDto>.Fail("Invoice not found.");
        }

        if (invoice.Status == InvoiceStatus.Paid)
        {
            return Result<PublicCheckoutSessionDto>.Fail("This invoice has already been paid.");
        }

        if (invoice.Status == InvoiceStatus.Cancelled)
        {
            return Result<PublicCheckoutSessionDto>.Fail("This invoice has been cancelled.");
        }

        var totalPaid = invoice.Payments.Sum(p => p.Amount.Amount);
        var amountDue = invoice.Total.Amount - totalPaid;
        var paymentAmount = req.Amount ?? amountDue;

        if (paymentAmount <= 0)
        {
            return Result<PublicCheckoutSessionDto>.Fail("Payment amount must be greater than zero.");
        }

        if (paymentAmount > amountDue)
        {
            return Result<PublicCheckoutSessionDto>.Fail(
                $"Payment amount cannot exceed amount due ({amountDue:C}).");
        }

        try
        {
            var session = await stripeConnectService.CreateConnectedCheckoutSessionAsync(
                new CheckoutSessionRequest
                {
                    ConnectedAccountId = tenant.StripeConnectedAccountId,
                    Amount = new Money { Amount = paymentAmount, Currency = invoice.Total.Currency },
                    SuccessUrl = req.SuccessUrl,
                    CancelUrl = req.CancelUrl,
                    LineItemDescription = $"Invoice #{invoice.Number}",
                    Metadata = new Dictionary<string, string>
                    {
                        [StripeMetadataKeys.TenantId] = tenant.Id.ToString(),
                        ["invoice_id"] = invoice.Id.ToString(),
                        ["payment_link_token"] = paymentLink.Token
                    }
                });

            logger.LogInformation(
                "Created public Checkout Session {SessionId} for invoice {InvoiceId}",
                session.Id, invoice.Id);

            return Result<PublicCheckoutSessionDto>.Ok(new PublicCheckoutSessionDto { Url = session.Url });
        }
        catch (Stripe.StripeException ex)
        {
            logger.LogError(ex, "Failed to create public Checkout Session for invoice {InvoiceId}", invoice.Id);
            return Result<PublicCheckoutSessionDto>.Fail($"Failed to start payment: {ex.Message}");
        }
    }
}
