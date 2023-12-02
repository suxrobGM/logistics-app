using Logistics.Shared;
using Logistics.Shared.Models;

namespace Logistics.Client.Abstractions;

public interface ISubscriptionApi
{
    Task<ResponseResult<SubscriptionDto>> GetSubscriptionAsync(string id);
    Task<ResponseResult<SubscriptionPlanDto>> GetSubscriptionPlanAsync(string planId);
    Task<PagedResponseResult<SubscriptionDto>> GetSubscriptionsAsync(PagedQuery query);
    Task<PagedResponseResult<SubscriptionPlanDto>> GetSubscriptionPlansAsync(PagedQuery query);
}
