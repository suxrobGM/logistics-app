using Logistics.Shared.Models;

namespace Logistics.HttpClient.Abstractions;

public interface ISubscriptionApi
{
    Task<Result<SubscriptionDto>> GetSubscriptionAsync(Guid id);
    Task<Result<SubscriptionPlanDto>> GetSubscriptionPlanAsync(Guid planId);
    Task<PagedResult<SubscriptionDto>> GetSubscriptionsAsync(PagedQuery query);
    Task<PagedResult<SubscriptionPlanDto>> GetSubscriptionPlansAsync(PagedQuery query);
    Task<Result> CreateSubscriptionPlanAsync(CreateSubscriptionPlan command);
    Task<Result> UpdateSubscriptionPlanAsync(UpdateSubscriptionPlan command);
    Task<Result> DeleteSubscriptionPlanAsync(Guid id);
    Task<Result> CreateSubscriptionAsync(CreateSubscription command);
    Task<Result> DeleteSubscriptionAsync(Guid id);
    Task<Result> CancelSubscriptionAsync(CancelSubscription command);
}
