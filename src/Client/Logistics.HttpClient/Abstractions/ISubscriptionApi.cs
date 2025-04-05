using Logistics.Shared.Models;

namespace Logistics.HttpClient.Abstractions;

public interface ISubscriptionApi
{
    Task<Result<SubscriptionDto>> GetSubscriptionAsync(string id);
    Task<Result<SubscriptionPlanDto>> GetSubscriptionPlanAsync(string planId);
    Task<PagedResult<SubscriptionDto>> GetSubscriptionsAsync(PagedQuery query);
    Task<PagedResult<SubscriptionPlanDto>> GetSubscriptionPlansAsync(PagedQuery query);
    Task<Result> CreateSubscriptionPlanAsync(CreateSubscriptionPlan command);
    Task<Result> UpdateSubscriptionPlanAsync(UpdateSubscriptionPlan command);
    Task<Result> DeleteSubscriptionPlanAsync(string id);
    Task<Result> CreateSubscriptionAsync(CreateSubscription command);
    Task<Result> DeleteSubscriptionAsync(string id);
}
