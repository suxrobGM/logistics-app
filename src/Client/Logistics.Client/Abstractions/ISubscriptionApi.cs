using Logistics.Client.Models;
using Logistics.Shared;
using Logistics.Shared.Models;

namespace Logistics.Client.Abstractions;

public interface ISubscriptionApi
{
    Task<ResponseResult<SubscriptionDto>> GetSubscriptionAsync(string id);
    Task<ResponseResult<SubscriptionPlanDto>> GetSubscriptionPlanAsync(string planId);
    Task<PagedResponseResult<SubscriptionDto>> GetSubscriptionsAsync(PagedQuery query);
    Task<PagedResponseResult<SubscriptionPlanDto>> GetSubscriptionPlansAsync(PagedQuery query);
    Task<ResponseResult> CreateSubscriptionPlanAsync(CreateSubscriptionPlan command);
    Task<ResponseResult> UpdateSubscriptionPlanAsync(UpdateSubscriptionPlan command);
    Task<ResponseResult> DeleteSubscriptionPlanAsync(string id);
    Task<ResponseResult> CreateSubscriptionAsync(CreateSubscription command);
    Task<ResponseResult> UpdateSubscriptionAsync(UpdateSubscription command);
    Task<ResponseResult> DeleteSubscriptionAsync(string id);
}
