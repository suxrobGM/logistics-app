using Logistics.Application.Abstractions;
using Logistics.Application.Modules.Financial.PaymentLinks.Services;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Domain.Primitives.Enums;
using Logistics.Domain.Primitives.ValueObjects;
using Logistics.Mediator;
using Logistics.Shared.Models;
using Microsoft.Extensions.Logging;
using Logistics.Application.Abstractions.Payments.Stripe;
using Logistics.Application.Abstractions.Payments;

namespace Logistics.Application.Modules.Financial.Payments.Commands;

internal sealed class CreatePublicCheckoutSessionHandler(
    IMasterUnitOfWork masterUow,
    ITenantUnitOfWork tenantUow,
    IPaymentLinkService paymentLinkService,
    IStripeConnectService stripeConnectService,
    ILogger<CreatePublicCheckoutSessionHandler> logger)
    : IRequestHandler<CreatePublicCheckoutSessionCommand, Result<PublicCheckoutSessionDto>>
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

        var found = await paymentLinkService.FindValidAsync(req.Token, ct);
        if (!found.IsSuccess)
        {
            return Result<PublicCheckoutSessionDto>.Fail(found.Error!);
        }

        var paymentLink = found.Value!;

        var invoice = await tenantUow.Repository<Invoice>().GetByIdAsync(paymentLink.InvoiceId, ct);
        if (invoice is null)
        {
            return Result<PublicCheckoutSessionDto>.Fail("Invoice not found.");
        }

        if (invoice is LoadInvoice { AssignedFactor: { } factor })
        {
            return Result<PublicCheckoutSessionDto>.Fail(
                $"This invoice was assigned to {factor.Name}. Please pay {factor.Name} directly.");
        }

        var paymentAmount = req.Amount ?? invoice.AmountDue;
        if (invoice.PaymentBlockedReason(paymentAmount) is { } paymentBlocked)
        {
            return Result<PublicCheckoutSessionDto>.Fail(paymentBlocked);
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
                        [StripeMetadataKeys.InvoiceId] = invoice.Id.ToString(),
                        [StripeMetadataKeys.PaymentLinkToken] = paymentLink.Token
                    }
                });

            logger.LogInformation(
                "Created public Checkout Session {SessionId} for invoice {InvoiceId}",
                session.SessionId, invoice.Id);

            return Result<PublicCheckoutSessionDto>.Ok(new PublicCheckoutSessionDto { Url = session.Url });
        }
        catch (StripeApiException ex)
        {
            logger.LogError(ex, "Failed to create public Checkout Session for invoice {InvoiceId}", invoice.Id);
            return Result<PublicCheckoutSessionDto>.Fail($"Failed to start payment: {ex.Message}");
        }
    }
}
