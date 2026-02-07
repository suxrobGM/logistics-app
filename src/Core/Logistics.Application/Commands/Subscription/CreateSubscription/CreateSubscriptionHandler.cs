using Logistics.Application.Abstractions;
using Logistics.Application.Services;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Shared.Models;
using Microsoft.Extensions.Logging;

namespace Logistics.Application.Commands;

internal sealed class CreateSubscriptionHandler : IAppRequestHandler<CreateSubscriptionCommand, Result>
{
    private readonly ILogger<CreateSubscriptionHandler> _logger;
    private readonly IMasterUnitOfWork _masterUow;
    private readonly IStripeService _stripeService;
    private readonly ITenantUnitOfWork _tenantUow;

    public CreateSubscriptionHandler(
        IMasterUnitOfWork masterUow,
        ITenantUnitOfWork tenantUow,
        IStripeService stripeService,
        ILogger<CreateSubscriptionHandler> logger)
    {
        _masterUow = masterUow;
        _tenantUow = tenantUow;
        _stripeService = stripeService;
        _logger = logger;
    }

    public async Task<Result> Handle(
        CreateSubscriptionCommand req, CancellationToken ct)
    {
        var tenant = await _tenantUow.SetCurrentTenantByIdAsync(req.TenantId);
        var truckCount = await _tenantUow.Repository<Truck>().CountAsync(ct: ct);

        if (tenant.StripeCustomerId is null)
        {
            await CreateStripeCustomerAsync(tenant);
        }

        var subscriptionPlan = await _masterUow.Repository<SubscriptionPlan>().GetByIdAsync(req.PlanId, ct);

        if (subscriptionPlan is null)
        {
            return Result.Fail($"Could not find a subscription plan with ID '{req.TenantId}'");
        }

        var subscription = Subscription.CreateTrial(tenant, subscriptionPlan);
        var stripeSubscription =
            await _stripeService.CreateSubscriptionAsync(subscriptionPlan, tenant, truckCount, true);
        subscription.StripeSubscriptionId = stripeSubscription.Id;

        await _masterUow.Repository<Subscription>().AddAsync(subscription, ct);
        await _masterUow.SaveChangesAsync(ct);
        _logger.LogInformation("Created Subscription for tenant {TenantId}, truck count: {TruckCount}", tenant.Id,
            truckCount);
        return Result.Ok();
    }

    private async Task CreateStripeCustomerAsync(Tenant tenant)
    {
        var stripeCustomer = await _stripeService.CreateCustomerAsync(tenant);
        tenant.StripeCustomerId = stripeCustomer.Id;
        _masterUow.Repository<Tenant>().Update(tenant);
        await _masterUow.SaveChangesAsync();
    }
}
