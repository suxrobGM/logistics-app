using Logistics.Application.Abstractions;
using Logistics.Application.Services;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Domain.Primitives.Enums;
using Logistics.Domain.Primitives.ValueObjects;
using Logistics.Shared.Models;
using Microsoft.Extensions.Logging;
using Stripe;
using StripeInvoice = Stripe.Invoice;
using StripeSubscription = Stripe.Subscription;
using Subscription = Logistics.Domain.Entities.Subscription;

namespace Logistics.Application.Commands;

internal sealed class ProcessStripEventHandler(
    ITenantUnitOfWork tenantUow,
    IMasterUnitOfWork masterUow,
    IStripeService stripeService,
    IStripeCustomerService stripeCustomerService,
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
            BillingAddress = stripeInvoice.CustomerAddress.ToAddressEntity()
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
}
