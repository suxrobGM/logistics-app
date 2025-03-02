using Logistics.Application.Utilities;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Shared.Models;

namespace Logistics.Application.Commands;

internal sealed class UpdateSubscriptionPlanHandler : RequestHandler<UpdateSubscriptionPlanCommand, Result>
{
    private readonly IMasterUnityOfWork _masterUow;

    public UpdateSubscriptionPlanHandler(IMasterUnityOfWork masterUow)
    {
        _masterUow = masterUow;
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
        
        _masterUow.Repository<SubscriptionPlan>().Update(subscriptionPlan);
        await _masterUow.SaveChangesAsync();
        return Result.Succeed();
    }
}
