using Logistics.Shared.Consts;
using Logistics.Shared.Models;
using Microsoft.AspNetCore.Components;

namespace Logistics.AdminApp.Components.Pages.Subscription;

public partial class AddSubscription : PageBase
{
    private SubscriptionDto _subscription = new();
    private IEnumerable<SubscriptionPlanDto>? _subscriptionPlans;
    private IEnumerable<TenantDto>? _tenants;
    

    #region Injectable services

    [Inject] 
    private NavigationManager Navigation { get; set; } = null!;

    #endregion
    

    protected override async Task OnInitializedAsync()
    {
        await FetchSubscriptionPlansAsync();
        await FetchTenantsAsync();
    }

    private async Task FetchSubscriptionPlansAsync()
    {
        var pagedData = await CallApiAsync(api => api.GetSubscriptionPlansAsync(new PagedQuery { PageSize = 100 }));
        _subscriptionPlans = pagedData?.Items;
    }
    
    private async Task FetchTenantsAsync()
    {
        var pagedData = await CallApiAsync(api => api.GetTenantsAsync(new SearchableQuery { PageSize = 100 }));
        _tenants = pagedData?.Items;
    }

    private async Task SubmitAsync(SubscriptionDto model)
    {
        var success = await CallApiAsync(api => api.CreateSubscriptionAsync(new CreateSubscription
        {
            Status = SubscriptionStatus.Active, // model.Status,
            PlanId = model.Plan?.Id,
            TenantId = model.Tenant?.Id
        }));

        if (success)
        {
            ShowNotification("A new subscription has been created successfully");
            ResetData();
            Navigation.NavigateTo("/subscriptions");
        }
    }

    private void ResetData()
    {
        _subscription = new SubscriptionDto();
    }
}
