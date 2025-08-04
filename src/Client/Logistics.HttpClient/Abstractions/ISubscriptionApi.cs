using Logistics.Shared.Models;

namespace Logistics.HttpClient.Abstractions;

public interface ISubscriptionApi
{
    Task<Result<SubscriptionDto>> GetSubscriptionAsync(Guid id);
    Task<Result<SubscriptionPlanDto>> GetSubscriptionPlanAsync(Guid planId);
    Task<PagedResult<SubscriptionDto>> GetSubscriptionsAsync(PagedQuery query);
    Task<PagedResult<SubscriptionPlanDto>> GetSubscriptionPlansAsync(PagedQuery query);
    Task<Result> CreateSubscriptionPlanAsync(CreateSubscriptionPlanCommand command);
    Task<Result> UpdateSubscriptionPlanAsync(UpdateSubscriptionPlanCommand command);
    Task<Result> DeleteSubscriptionPlanAsync(Guid id);
    Task<Result> CreateSubscriptionAsync(CreateSubscriptionCommand command);
    Task<Result> DeleteSubscriptionAsync(Guid id);
    Task<Result> CancelSubscriptionAsync(CancelSubscriptionCommand command);
}
