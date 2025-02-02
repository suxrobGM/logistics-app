using Logistics.Application;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Domain.Services;
using Logistics.Shared;

namespace Logistics.Application.Admin.Commands;

internal sealed class DeleteSubscriptionPlanHandler : RequestHandler<DeleteSubscriptionPlanCommand, Result>
{
    private readonly IMasterUnityOfWork _masterUow;

    public DeleteSubscriptionPlanHandler(IMasterUnityOfWork masterUow)
    {
        _masterUow = masterUow;
    }

    protected override async Task<Result> HandleValidated(
        DeleteSubscriptionPlanCommand req, CancellationToken cancellationToken)
    {
        var subscriptionPlan = await _masterUow.Repository<SubscriptionPlan>().GetByIdAsync(req.Id);

        if (subscriptionPlan is null)
        {
            return Result.Fail($"Could not find a subscription plan with ID '{req.Id}'");
        }

        _masterUow.Repository<SubscriptionPlan>().Delete(subscriptionPlan);
        await _masterUow.SaveChangesAsync();
        return Result.Succeed();
    }
}
