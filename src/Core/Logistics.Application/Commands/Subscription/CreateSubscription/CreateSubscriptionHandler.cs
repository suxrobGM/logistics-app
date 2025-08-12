using Logistics.Application.Services;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Shared.Models;
using Microsoft.Extensions.Logging;

namespace Logistics.Application.Commands;

internal sealed class CreateSubscriptionHandler : RequestHandler<CreateSubscriptionCommand, Result>
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

    protected override async Task<Result> HandleValidated(
        CreateSubscriptionCommand req, CancellationToken ct)
    {
        var tenant = await _masterUow.Repository<Tenant>().GetByIdAsync(req.TenantId);

        if (tenant is null)
        {
            return Result.Fail($"Could not find a tenant with ID '{req.TenantId}'");
        }

        _tenantUow.SetCurrentTenant(tenant);
        var tenantEmployeeCount = await _tenantUow.Repository<Employee>().CountAsync();

        if (tenant.StripeCustomerId is null)
        {
            await CreateStripeCustomerAsync(tenant);
        }

        var subscriptionPlan = await _masterUow.Repository<SubscriptionPlan>().GetByIdAsync(req.PlanId);

        if (subscriptionPlan is null)
        {
            return Result.Fail($"Could not find a subscription plan with ID '{req.TenantId}'");
        }

        var subscription = Subscription.CreateTrial(tenant, subscriptionPlan);
        var stripeSubscription =
            await _stripeService.CreateSubscriptionAsync(subscriptionPlan, tenant, tenantEmployeeCount, true);
        subscription.StripeSubscriptionId = stripeSubscription.Id;

        await _masterUow.Repository<Subscription>().AddAsync(subscription);
        await _masterUow.SaveChangesAsync();
        _logger.LogInformation("Created Subscription for tenant {TenantId}, employee count: {EmployeeCount}", tenant.Id,
            tenantEmployeeCount);
        return Result.Succeed();
    }

    private async Task CreateStripeCustomerAsync(Tenant tenant)
    {
        var stripeCustomer = await _stripeService.CreateCustomerAsync(tenant);
        tenant.StripeCustomerId = stripeCustomer.Id;
        _masterUow.Repository<Tenant>().Update(tenant);
        await _masterUow.SaveChangesAsync();
    }
}
