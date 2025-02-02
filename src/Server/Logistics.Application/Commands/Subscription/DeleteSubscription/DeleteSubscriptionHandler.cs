using Logistics.Application;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Domain.Services;
using Logistics.Shared;

namespace Logistics.Application.Admin.Commands;

internal sealed class DeleteSubscriptionHandler : RequestHandler<DeleteSubscriptionCommand, Result>
{
    private readonly IMasterUnityOfWork _masterUow;

    public DeleteSubscriptionHandler(IMasterUnityOfWork masterUow)
    {
        _masterUow = masterUow;
    }

    protected override async Task<Result> HandleValidated(
        DeleteSubscriptionCommand req, CancellationToken cancellationToken)
    {
        var subscription = await _masterUow.Repository<Subscription>().GetByIdAsync(req.Id);

        if (subscription is null)
        {
            return Result.Fail($"Could not find a subscription with ID '{req.Id}'");
        }

        _masterUow.Repository<Subscription>().Delete(subscription);
        await _masterUow.SaveChangesAsync();
        return Result.Succeed();
    }
}
