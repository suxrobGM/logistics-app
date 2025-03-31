using Logistics.Application.Services;
using Logistics.Application.Utilities;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Shared.Models;
using Microsoft.Extensions.Logging;

namespace Logistics.Application.Commands;

internal sealed class UpdateSubscriptionPlanHandler : RequestHandler<UpdateSubscriptionPlanCommand, Result>
{
    private readonly IMasterUnityOfWork _masterUow;
    private readonly IStripeService _stripeService;
    private readonly ILogger<UpdateSubscriptionPlanHandler> _logger;

    public UpdateSubscriptionPlanHandler(
        IMasterUnityOfWork masterUow, 
        IStripeService stripeService, 
        ILogger<UpdateSubscriptionPlanHandler> logger)
    {
        _masterUow = masterUow;
        _stripeService = stripeService;
        _logger = logger;
    }

    protected override async Task<Result> HandleValidated(
        UpdateSubscriptionPlanCommand req, CancellationToken cancellationToken)
    {
        var subscriptionPlan = await _masterUow.Repository<SubscriptionPlan>().GetByIdAsync(req.Id);

        if (subscriptionPlan is null)
        {
            return Result.Fail($"Could not find a subscription plan with ID '{req.Id}'");
        }

        subscriptionPlan.Name = PropertyUpdater.UpdateIfChanged(req.Name, subscriptionPlan.Name);
        subscriptionPlan.Description = PropertyUpdater.UpdateIfChanged(req.Description, subscriptionPlan.Description);
        subscriptionPlan.Price = PropertyUpdater.UpdateIfChanged(req.Price, subscriptionPlan.Price);
        subscriptionPlan.HasTrial = PropertyUpdater.UpdateIfChanged(req.HasTrial, subscriptionPlan.HasTrial);
        
        await _stripeService.UpdateSubscriptionPlanAsync(subscriptionPlan);
        _masterUow.Repository<SubscriptionPlan>().Update(subscriptionPlan);
        await _masterUow.SaveChangesAsync();
        _logger.LogInformation("Updated subscription plan {SubscriptionPlanId}", subscriptionPlan.Id);
        return Result.Succeed();
    }
}
