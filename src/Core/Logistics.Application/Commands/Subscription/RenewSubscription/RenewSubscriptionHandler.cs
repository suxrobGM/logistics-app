using Logistics.Application.Services;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Shared.Consts;
using Logistics.Shared.Models;
using Microsoft.Extensions.Logging;

namespace Logistics.Application.Commands;

internal sealed class RenewSubscriptionHandler : RequestHandler<RenewSubscriptionCommand, Result>
{
    private readonly IMasterUnityOfWork _masterUow;
    private readonly IStripeService _stripeService;
    private readonly ILogger<DeleteSubscriptionHandler> _logger;

    public RenewSubscriptionHandler(
        IMasterUnityOfWork masterUow, 
        IStripeService stripeService, 
        ILogger<DeleteSubscriptionHandler> logger)
    {
        _masterUow = masterUow;
        _stripeService = stripeService;
        _logger = logger;
    }

    protected override async Task<Result> HandleValidated(
        RenewSubscriptionCommand req, CancellationToken cancellationToken)
    {
        var subEntity = await _masterUow.Repository<Subscription>().GetByIdAsync(req.Id);

        if (subEntity is null)
        {
            return Result.Fail($"Could not find a subscription with ID '{req.Id}'");
        }
        
        var employeeCount = await _masterUow.Repository<User>().CountAsync(i => i.TenantId == subEntity.TenantId);

        _logger.LogInformation("Renewing stripe subscription {StripeSubscriptionId}", subEntity.StripeSubscriptionId);
        var subStripe = await _stripeService.RenewSubscriptionAsync(subEntity, subEntity.Plan, subEntity.Tenant, employeeCount);

        subEntity.StripeSubscriptionId = subStripe.Id;
        subEntity.Status = SubscriptionStatus.Active;
        subEntity.StartDate = subStripe.StartDate;
        subEntity.NextBillingDate = subStripe.CurrentPeriodEnd;
        subEntity.TrialEndDate = subStripe.TrialEnd;
        
        _masterUow.Repository<Subscription>().Update(subEntity);
        await _masterUow.SaveChangesAsync();
        _logger.LogInformation("Renewed subscription {SubscriptionId}", subEntity.Id);
        return Result.Succeed();
    }
}
