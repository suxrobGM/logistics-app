using Logistics.Application.Modules.Financial.StripeConnect.Services;
using Logistics.Application.Abstractions;
using Logistics.Application.Abstractions.Payments;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Domain.Primitives.Enums;
using Logistics.Domain.Primitives.ValueObjects;
using Logistics.Shared.Models;
using Microsoft.Extensions.Logging;
using Stripe;
using Stripe.Checkout;
using Logistics.Application.Abstractions.Payments.Stripe;
using DomainInvoice = Logistics.Domain.Entities.Invoice;
using StripeInvoice = Stripe.Invoice;
using StripeSubscription = Stripe.Subscription;
using Subscription = Logistics.Domain.Entities.Subscription;

namespace Logistics.Application.Modules.Integrations.Webhooks.Commands;

internal sealed class ProcessStripEventHandler(
    ITenantUnitOfWork tenantUow,
    IMasterUnitOfWork masterUow,
    IStripeService stripeService,
    IStripeCustomerService stripeCustomerService,
    IStripeConnectService stripeConnectService,
    IStripeAddressMapper stripeAddressMapper,
    ILogger<ProcessStripEventHandler> logger)
    : IAppRequestHandler<ProcessStripEventCommand, Result>
{
    public async Task<Result> Handle(
        ProcessStripEventCommand req, CancellationToken ct)
    {
        try
        {
            var stripeEvent = EventUtility.ConstructEvent(
                req.RequestBodyJson,
                req.StripeSignature,
                stripeService.WebhookSecret,
                throwOnApiVersionMismatch: false);

            logger.LogInformation("Received Stripe event: {Type}", stripeEvent.Type);

            return stripeEvent.Type switch
            {
                EventTypes.InvoicePaid => await HandleInvoicePaid((stripeEvent.Data.Object as StripeInvoice)!),
                EventTypes.CustomerSubscriptionCreated => await HandleSubscriptionCreated(
                    (stripeEvent.Data.Object as StripeSubscription)!),
                EventTypes.CustomerSubscriptionUpdated => await HandleSubscriptionUpdated(
                    (stripeEvent.Data.Object as StripeSubscription)!),
                EventTypes.CustomerSubscriptionDeleted => await HandleSubscriptionDeleted(
                    (stripeEvent.Data.Object as StripeSubscription)!),
                EventTypes.PaymentIntentProcessing => await HandlePaymentIntentStatus(
                    (stripeEvent.Data.Object as PaymentIntent)!, PaymentStatus.Pending),
                EventTypes.PaymentIntentSucceeded => await HandlePaymentIntentStatus(
                    (stripeEvent.Data.Object as PaymentIntent)!, PaymentStatus.Paid),
                EventTypes.PaymentIntentPaymentFailed => await HandlePaymentIntentFailed(
                    (stripeEvent.Data.Object as PaymentIntent)!),
                EventTypes.AccountUpdated => await HandleAccountUpdated(
                    (stripeEvent.Data.Object as Account)!),
                EventTypes.CheckoutSessionCompleted => await HandleCheckoutSessionCompleted(
                    (stripeEvent.Data.Object as Session)!),
                _ => Result.Ok()
            };
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Stripe error: {Message}", ex.Message);
            return Result.Fail(ex.Message);
        }
    }


    #region Invoice Handlers

    private async Task<Result> HandleInvoicePaid(StripeInvoice stripeInvoice)
    {
        var customer = await stripeCustomerService.GetCustomerAsync(stripeInvoice.CustomerId);
        if (!customer.Metadata.TryGetValue(StripeMetadataKeys.TenantId, out var tenantId))
        {
            return Result.Fail($"{StripeMetadataKeys.TenantId} not found in invoice metadata.");
        }

        if (!Guid.TryParse(tenantId, out var parsedTenantId))
        {
            return Result.Fail($"Invalid tenant ID format: {tenantId}");
        }

        await tenantUow.SetCurrentTenantByIdAsync(parsedTenantId);

        var amount = stripeInvoice.AmountPaid / 100m; // Convert from cents
        var tenant = tenantUow.GetCurrentTenant();

        var payment = new Payment
        {
            Amount = new Money { Amount = amount, Currency = stripeInvoice.Currency },
            StripePaymentMethodId = stripeInvoice.DefaultPaymentMethodId,
            TenantId = tenant.Id,
            Status = PaymentStatus.Paid,
            BillingAddress = stripeAddressMapper.ToAddress(stripeInvoice.CustomerAddress)
        };

        await tenantUow.Repository<Payment>().AddAsync(payment);
        await tenantUow.SaveChangesAsync();

        logger.LogInformation("Added payment for tenant {TenantId} with amount {Amount}, invoice ID {StripeInvoiceId}",
            tenantId, payment.Amount, stripeInvoice.Id);
        return Result.Ok();
    }

    #endregion


    #region Subscription Handlers

    private async Task<Result> HandleSubscriptionCreated(StripeSubscription stripeSubscription)
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

        var tenant = await masterUow.Repository<Tenant>().GetByIdAsync(Guid.Parse(tenantId));

        if (tenant is null)
        {
            return Result.Fail($"Could not find a tenant with ID '{tenantId}'");
        }

        var subscriptionPlan = await masterUow.Repository<SubscriptionPlan>().GetByIdAsync(Guid.Parse(planId));

        if (subscriptionPlan is null)
        {
            return Result.Fail($"Could not find a subscription plan with ID '{planId}'");
        }

        if (tenant.Subscription is not null)
        {
            tenant.Subscription.Status = StripeObjectMapper.GetSubscriptionStatus(stripeSubscription.Status);
            tenant.Subscription.StripeSubscriptionId = stripeSubscription.Id;
            tenant.Subscription.PlanId = subscriptionPlan.Id;

            masterUow.Repository<Subscription>().Update(tenant.Subscription);
            logger.LogInformation("Updated existing subscription {StripeSubscriptionId} for tenant {TenantId}",
                stripeSubscription.Id, tenantId);
        }
        else
        {
            var newSubscription = new Subscription
            {
                Status = StripeObjectMapper.GetSubscriptionStatus(stripeSubscription.Status),
                TenantId = tenant.Id,
                Tenant = tenant,
                PlanId = subscriptionPlan.Id,
                Plan = subscriptionPlan,
                StripeSubscriptionId = stripeSubscription.Id,
                StripeCustomerId = tenant.StripeCustomerId
            };

            await masterUow.Repository<Subscription>().AddAsync(newSubscription);
            logger.LogInformation("Created new subscription {StripeSubscriptionId} for tenant {TenantId}",
                stripeSubscription.Id, tenantId);
        }

        await masterUow.SaveChangesAsync();
        return Result.Ok();
    }

    private async Task<Result> HandleSubscriptionUpdated(StripeSubscription stripeSubscription)
    {
        var metadata = stripeSubscription.Metadata;
        if (!metadata.TryGetValue(StripeMetadataKeys.TenantId, out var tenantId))
        {
            return Result.Fail($"{StripeMetadataKeys.TenantId} not found in subscription metadata.");
        }

        var subscription = await masterUow.Repository<Subscription>()
            .GetAsync(s => s.StripeSubscriptionId == stripeSubscription.Id);

        if (subscription is null)
        {
            return Result.Fail($"Subscription {stripeSubscription.Id} not found for tenant {tenantId}.");
        }

        subscription.Status = StripeObjectMapper.GetSubscriptionStatus(stripeSubscription.Status);
        subscription.CancelAtPeriodEnd = stripeSubscription.CancelAtPeriodEnd;

        // Update plan if changed
        if (metadata.TryGetValue(StripeMetadataKeys.PlanId, out var planId)
            && Guid.TryParse(planId, out var parsedPlanId)
            && parsedPlanId != subscription.PlanId)
        {
            subscription.PlanId = parsedPlanId;
        }

        masterUow.Repository<Subscription>().Update(subscription);
        await masterUow.SaveChangesAsync();

        logger.LogInformation("Updated subscription {StripeSubscriptionId} for tenant {TenantId}",
            stripeSubscription.Id, tenantId);
        return Result.Ok();
    }

    private async Task<Result> HandleSubscriptionDeleted(StripeSubscription stripeSubscription)
    {
        if (!stripeSubscription.Metadata.TryGetValue(StripeMetadataKeys.TenantId, out var tenantId))
        {
            return Result.Fail($"{StripeMetadataKeys.TenantId} not found in subscription metadata.");
        }

        var subscription = await masterUow.Repository<Subscription>()
            .GetAsync(s => s.StripeSubscriptionId == stripeSubscription.Id);

        if (subscription is null)
        {
            return Result.Fail($"Subscription {stripeSubscription.Id} not found for tenant {tenantId}.");
        }

        subscription.Status = StripeObjectMapper.GetSubscriptionStatus(stripeSubscription.Status);
        masterUow.Repository<Subscription>().Update(subscription);
        await masterUow.SaveChangesAsync();

        logger.LogInformation("Cancelled subscription {StripeSubscriptionId} for tenant {TenantId}",
            stripeSubscription.Id, tenantId);
        return Result.Ok();
    }

    #endregion


    #region PaymentIntent Handlers

    private async Task<Result> HandlePaymentIntentStatus(PaymentIntent paymentIntent, PaymentStatus newStatus)
    {
        // Subscription billing flows through invoice.paid which already creates the Payment row
        // (without StripePaymentIntentId). The lookup below naturally skips those events because
        // no row matches the intent ID.
        if (!TryGetTenantId(paymentIntent.Metadata, out var tenantId))
        {
            return Result.Fail($"{StripeMetadataKeys.TenantId} not found in PaymentIntent metadata.");
        }

        await tenantUow.SetCurrentTenantByIdAsync(tenantId);

        var payment = await tenantUow.Repository<Payment>()
            .GetAsync(p => p.StripePaymentIntentId == paymentIntent.Id);

        if (payment is null)
        {
            return Result.Ok();
        }

        // Idempotency: don't downgrade a terminal status (Paid/Failed/Cancelled) back to Pending.
        if (payment.Status == newStatus || IsTerminal(payment.Status))
        {
            return Result.Ok();
        }

        payment.Status = newStatus;
        tenantUow.Repository<Payment>().Update(payment);
        await tenantUow.SaveChangesAsync();

        logger.LogInformation(
            "Updated payment {PaymentId} to {Status} from PaymentIntent {PaymentIntentId}",
            payment.Id, newStatus, paymentIntent.Id);
        return Result.Ok();
    }

    private async Task<Result> HandlePaymentIntentFailed(PaymentIntent paymentIntent)
    {
        if (!TryGetTenantId(paymentIntent.Metadata, out var tenantId))
        {
            return Result.Fail($"{StripeMetadataKeys.TenantId} not found in PaymentIntent metadata.");
        }

        await tenantUow.SetCurrentTenantByIdAsync(tenantId);

        var payment = await tenantUow.Repository<Payment>()
            .GetAsync(p => p.StripePaymentIntentId == paymentIntent.Id);

        if (payment is null)
        {
            return Result.Ok();
        }

        if (IsTerminal(payment.Status))
        {
            return Result.Ok();
        }

        payment.Status = PaymentStatus.Failed;
        var errorMessage = paymentIntent.LastPaymentError?.Message;
        if (!string.IsNullOrEmpty(errorMessage))
        {
            payment.Description = string.IsNullOrEmpty(payment.Description)
                ? $"Failed: {errorMessage}"
                : $"{payment.Description} | Failed: {errorMessage}";
        }

        tenantUow.Repository<Payment>().Update(payment);
        await tenantUow.SaveChangesAsync();

        logger.LogInformation(
            "Marked payment {PaymentId} as Failed from PaymentIntent {PaymentIntentId}: {Error}",
            payment.Id, paymentIntent.Id, errorMessage);
        return Result.Ok();
    }

    private static bool IsTerminal(PaymentStatus status) =>
        status is PaymentStatus.Paid
            or PaymentStatus.Failed
            or PaymentStatus.Cancelled
            or PaymentStatus.Refunded;

    private static bool TryGetTenantId(IDictionary<string, string> metadata, out Guid tenantId)
    {
        tenantId = Guid.Empty;
        return metadata.TryGetValue(StripeMetadataKeys.TenantId, out var raw)
            && Guid.TryParse(raw, out tenantId);
    }

    #endregion


    #region Checkout Handlers

    private async Task<Result> HandleCheckoutSessionCompleted(Session session)
    {
        if (!TryGetTenantId(session.Metadata, out var tenantId))
        {
            return Result.Ok();
        }

        if (!session.Metadata.TryGetValue("invoice_id", out var rawInvoiceId)
            || !Guid.TryParse(rawInvoiceId, out var invoiceId))
        {
            return Result.Fail("invoice_id missing or invalid in Checkout Session metadata.");
        }

        await tenantUow.SetCurrentTenantByIdAsync(tenantId);

        var invoice = await tenantUow.Repository<DomainInvoice>().GetByIdAsync(invoiceId);
        if (invoice is null)
        {
            return Result.Fail($"Invoice {invoiceId} not found for tenant {tenantId}.");
        }

        // Idempotency: Stripe retries delivered events. Bail if we already recorded this session.
        var paymentIntentId = session.PaymentIntentId;
        if (!string.IsNullOrEmpty(paymentIntentId))
        {
            var existing = await tenantUow.Repository<Payment>()
                .GetAsync(p => p.StripePaymentIntentId == paymentIntentId);
            if (existing is not null)
            {
                return Result.Ok();
            }
        }

        // SEPA / BACS settle 1Ã¢â‚¬â€œ3 business days after the session completes; Stripe surfaces this
        // as the PaymentIntent status. Mirror that on the local row.
        var status = session.PaymentStatus switch
        {
            "paid" => PaymentStatus.Paid,
            "no_payment_required" => PaymentStatus.Paid,
            _ => PaymentStatus.Pending
        };

        var amount = (session.AmountTotal ?? 0) / 100m;
        var currency = (session.Currency ?? invoice.Total.Currency).ToUpperInvariant();

        var payment = new Payment
        {
            Amount = new Money { Amount = amount, Currency = currency },
            Status = status,
            TenantId = tenantId,
            StripePaymentIntentId = paymentIntentId,
            // Stripe's billing details aren't expanded onto the session payload; reuse the
            // tenant's company address as a placeholder, matching ProcessPublicPaymentHandler.
            BillingAddress = (await masterUow.Repository<Tenant>().GetByIdAsync(tenantId))!.CompanyAddress,
            Description = $"Payment for Invoice #{invoice.Number}"
        };

        await tenantUow.Repository<Payment>().AddAsync(payment);

        // Only apply the payment to the invoice once it actually settles. Pending SEPA payments
        // remain unlinked until payment_intent.succeeded fires.
        if (status == PaymentStatus.Paid)
        {
            invoice.ApplyPayment(payment);
            tenantUow.Repository<DomainInvoice>().Update(invoice);
        }

        await tenantUow.SaveChangesAsync();

        logger.LogInformation(
            "Recorded payment {PaymentId} from Checkout Session {SessionId} for invoice {InvoiceId} (status: {Status})",
            payment.Id, session.Id, invoice.Id, status);
        return Result.Ok();
    }

    #endregion


    #region Account Handlers

    private async Task<Result> HandleAccountUpdated(Account account)
    {
        if (!account.Metadata.TryGetValue(StripeMetadataKeys.TenantId, out var tenantId)
            || !Guid.TryParse(tenantId, out var parsedTenantId))
        {
            // Connect accounts created outside the tenant flow (e.g., employee payout accounts)
            // don't carry tenant_id metadata Ã¢â‚¬â€ silently ignore.
            return Result.Ok();
        }

        var tenant = await masterUow.Repository<Tenant>().GetByIdAsync(parsedTenantId);
        if (tenant is null || tenant.StripeConnectedAccountId != account.Id)
        {
            return Result.Ok();
        }

        await stripeConnectService.SyncConnectedAccountStatusAsync(tenant);
        masterUow.Repository<Tenant>().Update(tenant);
        await masterUow.SaveChangesAsync();

        logger.LogInformation(
            "Synced Connect status for tenant {TenantId} from account.updated webhook",
            tenant.Id);
        return Result.Ok();
    }

    #endregion
}
