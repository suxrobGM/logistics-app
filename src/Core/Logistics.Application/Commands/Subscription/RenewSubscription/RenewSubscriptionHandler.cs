﻿using Logistics.Application.Services;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Shared.Models;
using Microsoft.Extensions.Logging;

namespace Logistics.Application.Commands;

internal sealed class RenewSubscriptionHandler : RequestHandler<RenewSubscriptionCommand, Result>
{
    private readonly IMasterUnityOfWork _masterUow;
    private readonly IStripeService _stripeService;
    private readonly ILogger<RenewSubscriptionHandler> _logger;

    public RenewSubscriptionHandler(
        IMasterUnityOfWork masterUow, 
        IStripeService stripeService, 
        ILogger<RenewSubscriptionHandler> logger)
    {
        _masterUow = masterUow;
        _stripeService = stripeService;
        _logger = logger;
    }

    protected override async Task<Result> HandleValidated(
        RenewSubscriptionCommand req, CancellationToken cancellationToken)
    {
        var subscription = await _masterUow.Repository<Subscription>().GetByIdAsync(req.Id);

        if (subscription is null)
        {
            return Result.Fail($"Could not find a subscription with ID '{req.Id}'");
        }
        
        var employeeCount = await _masterUow.Repository<User>().CountAsync(i => i.TenantId == subscription.TenantId);

        _logger.LogInformation("Renewing stripe subscription {StripeSubscriptionId}", subscription.StripeSubscriptionId);
        var stripeSubscription = await _stripeService.RenewSubscriptionAsync(subscription, subscription.Plan, subscription.Tenant, employeeCount);
        subscription.StripeSubscriptionId = stripeSubscription.Id;
        subscription.Status = StripeObjectMapper.GetSubscriptionStatus(stripeSubscription.Status);
        subscription.StartDate = stripeSubscription.StartDate;
        subscription.NextBillingDate = stripeSubscription.Items.Data.First().CurrentPeriodEnd;
        subscription.TrialEndDate = stripeSubscription.TrialEnd;
        
        _masterUow.Repository<Subscription>().Update(subscription);
        await _masterUow.SaveChangesAsync();
        _logger.LogInformation("Renewed subscription {SubscriptionId}", subscription.Id);
        return Result.Succeed();
    }
}
