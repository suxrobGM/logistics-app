using Logistics.Shared.Models;

namespace Logistics.HttpClient.Abstractions;

public interface ISubscriptionApi
{
    Task<SubscriptionDto?> GetSubscriptionAsync(Guid id);
    Task<SubscriptionPlanDto?> GetSubscriptionPlanAsync(Guid planId);
    Task<PagedResponse<SubscriptionDto>?> GetSubscriptionsAsync(PagedQuery query);
    Task<PagedResponse<SubscriptionPlanDto>?> GetSubscriptionPlansAsync(PagedQuery query);
    Task<bool> CreateSubscriptionPlanAsync(CreateSubscriptionPlanCommand command);
    Task<bool> UpdateSubscriptionPlanAsync(UpdateSubscriptionPlanCommand command);
    Task<bool> DeleteSubscriptionPlanAsync(Guid id);
    Task<bool> CreateSubscriptionAsync(CreateSubscriptionCommand command);
    Task<bool> DeleteSubscriptionAsync(Guid id);
    Task<bool> CancelSubscriptionAsync(CancelSubscriptionCommand command);
}
