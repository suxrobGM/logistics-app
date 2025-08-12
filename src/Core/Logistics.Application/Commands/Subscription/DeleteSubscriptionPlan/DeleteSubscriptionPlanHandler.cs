using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Shared.Models;

namespace Logistics.Application.Commands;

internal sealed class DeleteSubscriptionPlanHandler : RequestHandler<DeleteSubscriptionPlanCommand, Result>
{
    private readonly IMasterUnityOfWork _masterUow;

    public DeleteSubscriptionPlanHandler(IMasterUnityOfWork masterUow)
    {
        _masterUow = masterUow;
    }

    protected override async Task<Result> HandleValidated(
        DeleteSubscriptionPlanCommand req, CancellationToken ct)
    {
        var subscriptionPlan = await _masterUow.Repository<SubscriptionPlan>().GetByIdAsync(req.Id);

        if (subscriptionPlan is null) return Result.Fail($"Could not find a subscription plan with ID '{req.Id}'");

        _masterUow.Repository<SubscriptionPlan>().Delete(subscriptionPlan);
        await _masterUow.SaveChangesAsync();
        return Result.Succeed();
    }
}
