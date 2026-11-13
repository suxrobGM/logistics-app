using Logistics.Application.Abstractions.Payments;
using Logistics.Application.Modules.Financial.StripeConnect.Services;
using Logistics.Application.Modules.Integrations.Webhooks.Services;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Domain.Primitives.Enums;
using Logistics.Domain.Primitives.ValueObjects;
using Logistics.Mediator;
using Logistics.Shared.Models;
using Microsoft.Extensions.Logging;

namespace Logistics.Application.Modules.Integrations.Webhooks.Commands;

internal sealed class ProcessStripeEventHandler(
    ITenantUnitOfWork tenantUow,
    IMasterUnitOfWork masterUow,
    IStripeWebhookParser webhookParser,
    IStripeConnectService stripeConnectService,
    IWebhookEventTracker webhookEvents,
    ILogger<ProcessStripeEventHandler> logger)
    : IRequestHandler<ProcessStripeEventCommand, Result>
{
    private const string Provider = "Stripe";

    public async Task<Result> Handle(ProcessStripeEventCommand req, CancellationToken ct)
    {
        try
        {
            var stripeEvent = webhookParser.Parse(req.RequestBodyJson, req.StripeSignature);
            if (stripeEvent is null)
            {
                return Result.Fail("Stripe webhook signature verification failed", ErrorCodes.WebhookRejected);
            }

            logger.LogInformation("Received Stripe event: {Type}", stripeEvent.Type);

            if (await webhookEvents.WasAlreadyHandledAsync(Provider, stripeEvent.Id, ct))
            {
                logger.LogInformation("Duplicate Stripe event '{EventId}' ignored", stripeEvent.Id);
                return Result.Ok();
            }

            var result = stripeEvent.Kind switch
            {
                StripeWebhookEventKind.SubscriptionCreated => await HandleSubscriptionCreated(stripeEvent.Subscription!, ct),
                StripeWebhookEventKind.SubscriptionUpdated => await HandleSubscriptionUpdated(stripeEvent.Subscription!, ct),
                StripeWebhookEventKind.SubscriptionDeleted => await HandleSubscriptionDeleted(stripeEvent.Subscription!, ct),
                StripeWebhookEventKind.PaymentIntentProcessing => await UpdatePaymentStatus(
                    stripeEvent.PaymentIntent!, PaymentStatus.Pending, ct),
                StripeWebhookEventKind.PaymentIntentSucceeded => await UpdatePaymentStatus(
                    stripeEvent.PaymentIntent!, PaymentStatus.Paid, ct),
                StripeWebhookEventKind.PaymentIntentFailed => await UpdatePaymentStatus(
                    stripeEvent.PaymentIntent!, PaymentStatus.Failed, ct),
                StripeWebhookEventKind.AccountUpdated => await HandleAccountUpdated(stripeEvent.Account!, ct),
                StripeWebhookEventKind.CheckoutSessionCompleted => await HandleCheckoutSessionCompleted(
                    stripeEvent.CheckoutSession!, ct),
                _ => Result.Ok()
            };

            if (result.IsSuccess)
            {
                await webhookEvents.MarkHandledAsync(Provider, stripeEvent.Id, ct);
            }

            return result;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Stripe error: {Message}", ex.Message);
            return Result.Fail("Could not process the Stripe event.");
        }
    }

    private async Task<Result> HandleSubscriptionCreated(StripeSubscriptionInfo stripeSubscription, CancellationToken ct)
    {
        var metadata = stripeSubscription.Metadata;
        if (!metadata.TryGetValue(StripeMetadataKeys.TenantId, out var tenantId))
        {
            return Result.Fail($"{StripeMetadataKeys.TenantId} not found in subscription metadata.");
        }

        if (!metadata.TryGetValue(StripeMetadataKeys.PlanId, out var planId))
        {
            return Result.Fail($"{StripeMetadataKeys.PlanId} not found in subscription metadata.");
        }

        var tenant = await masterUow.Repository<Tenant>().GetByIdAsync(Guid.Parse(tenantId), ct);

        if (tenant is null)
        {
            return Result.Fail($"Could not find a tenant with ID '{tenantId}'");
        }

        var subscriptionPlan = await masterUow.Repository<SubscriptionPlan>().GetByIdAsync(Guid.Parse(planId), ct);

        if (subscriptionPlan is null)
        {
            return Result.Fail($"Could not find a subscription plan with ID '{planId}'");
        }

        var status = StripeObjectMapper.GetSubscriptionStatus(stripeSubscription.Status);

        if (tenant.Subscription is not null)
        {
            tenant.Subscription.Status = status;
            tenant.Subscription.StripeSubscriptionId = stripeSubscription.Id;
            tenant.Subscription.PlanId = subscriptionPlan.Id;

            logger.LogInformation("Updated existing subscription {StripeSubscriptionId} for tenant {TenantId}",
                stripeSubscription.Id, tenantId);
        }
        else
        {
            var newSubscription = new Subscription
            {
                Status = status,
                TenantId = tenant.Id,
                Tenant = tenant,
                PlanId = subscriptionPlan.Id,
                Plan = subscriptionPlan,
                StripeSubscriptionId = stripeSubscription.Id
            };

            await masterUow.Repository<Subscription>().AddAsync(newSubscription, ct);
            logger.LogInformation("Created new subscription {StripeSubscriptionId} for tenant {TenantId}",
                stripeSubscription.Id, tenantId);
        }

        await masterUow.SaveChangesAsync(ct);
        return Result.Ok();
    }

    private async Task<Result> HandleSubscriptionUpdated(StripeSubscriptionInfo stripeSubscription, CancellationToken ct)
    {
        var metadata = stripeSubscription.Metadata;
        if (!metadata.TryGetValue(StripeMetadataKeys.TenantId, out var tenantId))
        {
            return Result.Fail($"{StripeMetadataKeys.TenantId} not found in subscription metadata.");
        }

        var subscription = await masterUow.Repository<Subscription>()
            .GetAsync(s => s.StripeSubscriptionId == stripeSubscription.Id, ct);

        if (subscription is null)
        {
            return Result.Fail($"Subscription {stripeSubscription.Id} not found for tenant {tenantId}.");
        }

        subscription.Status = StripeObjectMapper.GetSubscriptionStatus(stripeSubscription.Status);
        subscription.CancelAtPeriodEnd = stripeSubscription.CancelAtPeriodEnd;

        if (TryGetGuid(metadata, StripeMetadataKeys.PlanId, out var planId))
        {
            subscription.PlanId = planId;
        }

        await masterUow.SaveChangesAsync(ct);

        logger.LogInformation("Updated subscription {StripeSubscriptionId} for tenant {TenantId}",
            stripeSubscription.Id, tenantId);
        return Result.Ok();
    }

    private async Task<Result> HandleSubscriptionDeleted(StripeSubscriptionInfo stripeSubscription, CancellationToken ct)
    {
        if (!stripeSubscription.Metadata.TryGetValue(StripeMetadataKeys.TenantId, out var tenantId))
        {
            return Result.Fail($"{StripeMetadataKeys.TenantId} not found in subscription metadata.");
        }

        var subscription = await masterUow.Repository<Subscription>()
            .GetAsync(s => s.StripeSubscriptionId == stripeSubscription.Id, ct);

        if (subscription is null)
        {
            return Result.Fail($"Subscription {stripeSubscription.Id} not found for tenant {tenantId}.");
        }

        subscription.Status = StripeObjectMapper.GetSubscriptionStatus(stripeSubscription.Status);
        await masterUow.SaveChangesAsync(ct);

        logger.LogInformation("Cancelled subscription {StripeSubscriptionId} for tenant {TenantId}",
            stripeSubscription.Id, tenantId);
        return Result.Ok();
    }

    private async Task<Result> UpdatePaymentStatus(StripePaymentIntentInfo paymentIntent, PaymentStatus newStatus, CancellationToken ct)
    {
        if (!TryGetGuid(paymentIntent.Metadata, StripeMetadataKeys.TenantId, out var tenantId))
        {
            return Result.Fail($"{StripeMetadataKeys.TenantId} not found in PaymentIntent metadata.");
        }

        await tenantUow.SetCurrentTenantByIdAsync(tenantId);

        var payment = await tenantUow.Repository<Payment>()
            .GetAsync(p => p.StripePaymentIntentId == paymentIntent.Id, ct);
        if (payment is null)
        {
            return Result.Ok();
        }

        // Stripe delivers at least once and out of order, so a terminal status
        // (Paid/Failed/Cancelled/Refunded) is never overwritten.
        if (payment.Status == newStatus || IsTerminal(payment.Status))
        {
            return Result.Ok();
        }

        payment.Status = newStatus;
        payment.Invoice?.RecalculatePaymentStatus(DateTime.UtcNow);

        var errorMessage = paymentIntent.LastErrorMessage;
        var recordFailureReason = newStatus == PaymentStatus.Failed && !string.IsNullOrEmpty(errorMessage);
        if (recordFailureReason)
        {
            payment.Description = string.IsNullOrEmpty(payment.Description)
                ? $"Failed: {errorMessage}"
                : $"{payment.Description} | Failed: {errorMessage}";
        }

        await tenantUow.SaveChangesAsync(ct);

        logger.LogInformation(
            "Updated payment {PaymentId} to {Status} from PaymentIntent {PaymentIntentId}",
            payment.Id, newStatus, paymentIntent.Id);
        return Result.Ok();
    }

    private static bool IsTerminal(PaymentStatus status) =>
        status is PaymentStatus.Paid
            or PaymentStatus.Failed
            or PaymentStatus.Cancelled
            or PaymentStatus.Refunded;

    private static bool TryGetGuid(IReadOnlyDictionary<string, string> metadata, string key, out Guid value)
    {
        value = Guid.Empty;
        return metadata.TryGetValue(key, out var raw) && Guid.TryParse(raw, out value);
    }

    private async Task<Result> HandleCheckoutSessionCompleted(StripeCheckoutSessionInfo session, CancellationToken ct)
    {
        if (!TryGetGuid(session.Metadata, StripeMetadataKeys.TenantId, out var tenantId))
        {
            return Result.Ok();
        }

        if (!TryGetGuid(session.Metadata, StripeMetadataKeys.InvoiceId, out var invoiceId))
        {
            return Result.Fail("invoice_id missing or invalid in Checkout Session metadata.");
        }

        await tenantUow.SetCurrentTenantByIdAsync(tenantId);

        var invoice = await tenantUow.Repository<Invoice>().GetByIdAsync(invoiceId, ct);
        if (invoice is null)
        {
            return Result.Fail($"Invoice {invoiceId} not found for tenant {tenantId}.");
        }

        // Stripe retries delivered events. Bail if we already recorded this session.
        var paymentIntentId = session.PaymentIntentId;
        if (!string.IsNullOrEmpty(paymentIntentId))
        {
            var existing = await tenantUow.Repository<Payment>()
                .GetAsync(p => p.StripePaymentIntentId == paymentIntentId, ct);
            if (existing is not null)
            {
                return Result.Ok();
            }
        }

        // SEPA / BACS settle 1-3 business days after the session completes; Stripe surfaces this
        // as the PaymentIntent status. Mirror that on the local row.
        var status = session.PaymentStatus switch
        {
            "paid" => PaymentStatus.Paid,
            "no_payment_required" => PaymentStatus.Paid,
            _ => PaymentStatus.Pending
        };

        var tenant = (await masterUow.Repository<Tenant>().GetByIdAsync(tenantId, ct))!;
        if (session.Currency is { } sessionCurrency
            && !string.Equals(sessionCurrency, tenant.Currency, StringComparison.OrdinalIgnoreCase))
        {
            logger.LogWarning(
                "Stripe session for invoice {InvoiceId} was paid in {SessionCurrency}, not the company currency {TenantCurrency}",
                invoice.Id, sessionCurrency, tenant.Currency);
        }

        var payment = new Payment
        {
            Amount = session.AmountTotal,
            Status = status,
            StripePaymentIntentId = paymentIntentId,
            // Stripe's billing details aren't expanded onto the session payload; reuse the
            // tenant's company address as a placeholder, matching ProcessPublicPaymentHandler.
            BillingAddress = tenant.CompanyAddress,
            Description = $"Payment for Invoice #{invoice.Number}"
        };

        await tenantUow.Repository<Payment>().AddAsync(payment, ct);

        // A pending (ACH/SEPA) payment is linked now but pays nothing until payment_intent.succeeded
        // marks it Paid and recalculates the invoice.
        invoice.ApplyPayment(payment);

        await tenantUow.SaveChangesAsync(ct);

        logger.LogInformation(
            "Recorded payment {PaymentId} from Checkout Session {SessionId} for invoice {InvoiceId} (status: {Status})",
            payment.Id, session.Id, invoice.Id, status);
        return Result.Ok();
    }

    private async Task<Result> HandleAccountUpdated(StripeAccountInfo account, CancellationToken ct)
    {
        if (!TryGetGuid(account.Metadata, StripeMetadataKeys.TenantId, out var tenantId))
        {
            // Connect accounts created outside the tenant flow (e.g., employee payout accounts)
            // don't carry tenant_id metadata - silently ignore.
            return Result.Ok();
        }

        var tenant = await masterUow.Repository<Tenant>().GetByIdAsync(tenantId, ct);
        if (tenant is null || tenant.StripeConnectedAccountId != account.Id)
        {
            return Result.Ok();
        }

        await stripeConnectService.SyncConnectedAccountStatusAsync(tenant);
        await masterUow.SaveChangesAsync(ct);

        logger.LogInformation(
            "Synced Connect status for tenant {TenantId} from account.updated webhook",
            tenant.Id);
        return Result.Ok();
    }
}
