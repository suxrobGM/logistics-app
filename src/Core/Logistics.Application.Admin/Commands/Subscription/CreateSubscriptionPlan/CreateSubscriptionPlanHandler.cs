using Logistics.Application.Core;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Domain.Services;
using Logistics.Domain.ValueObjects;
using Logistics.Shared;

namespace Logistics.Application.Admin.Commands;

internal sealed class CreateSubscriptionPlanHandler : RequestHandler<CreateSubscriptionPlanCommand, Result>
{
    private readonly IMasterUnityOfWork _masterUow;

    public CreateSubscriptionPlanHandler(IMasterUnityOfWork masterUow)
    {
        _masterUow = masterUow;
    }

    protected override async Task<Result> HandleValidated(
        CreateSubscriptionPlanCommand req, CancellationToken cancellationToken)
    {
        var subscriptionPlan = new SubscriptionPlan
        {
            Name = req.Name,
            Description = req.Description,
            Price = req.Price
        };
        await _masterUow.Repository<SubscriptionPlan>().AddAsync(subscriptionPlan);
        await _masterUow.SaveChangesAsync();
        return Result.Succeed();
    }
}
