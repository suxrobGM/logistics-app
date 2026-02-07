using Logistics.Application.Abstractions;
using Logistics.Application.Services;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Shared.Models;
using Microsoft.Extensions.Logging;

namespace Logistics.Application.Commands;

internal sealed class CreateSubscriptionHandler(
    IMasterUnitOfWork masterUow,
    ITenantUnitOfWork tenantUow,
    IStripeSubscriptionService stripeSubscriptionService,
    IStripeCustomerService stripeCustomerService,
    ILogger<CreateSubscriptionHandler> logger) : IAppRequestHandler<CreateSubscriptionCommand, Result>
{
    public async Task<Result> Handle(
        CreateSubscriptionCommand req, CancellationToken ct)
    {
        var tenant = await tenantUow.SetCurrentTenantByIdAsync(req.TenantId);
        var truckCount = await tenantUow.Repository<Truck>().CountAsync(ct: ct);

        if (tenant.StripeCustomerId is null)
        {
            await CreateStripeCustomerAsync(tenant);
        }

        var subscriptionPlan = await masterUow.Repository<SubscriptionPlan>().GetByIdAsync(req.PlanId, ct);

        if (subscriptionPlan is null)
        {
            return Result.Fail($"Could not find a subscription plan with ID '{req.TenantId}'");
        }

        var subscription = Subscription.CreateTrial(tenant, subscriptionPlan);
        var stripeSubscription =
            await stripeSubscriptionService.CreateSubscriptionAsync(subscriptionPlan, tenant, truckCount, true);
        subscription.StripeSubscriptionId = stripeSubscription.Id;

        await masterUow.Repository<Subscription>().AddAsync(subscription, ct);
        await masterUow.SaveChangesAsync(ct);
        logger.LogInformation("Created Subscription for tenant {TenantId}, truck count: {TruckCount}", tenant.Id,
            truckCount);
        return Result.Ok();
    }

    private async Task CreateStripeCustomerAsync(Tenant tenant)
    {
        var stripeCustomer = await stripeCustomerService.CreateCustomerAsync(tenant);
        tenant.StripeCustomerId = stripeCustomer.Id;
        masterUow.Repository<Tenant>().Update(tenant);
        await masterUow.SaveChangesAsync();
    }
}
