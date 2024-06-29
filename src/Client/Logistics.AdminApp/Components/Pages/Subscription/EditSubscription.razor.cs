using Logistics.Client.Models;
using Logistics.Shared;
using Logistics.Shared.Consts;
using Logistics.Shared.Models;
using Microsoft.AspNetCore.Components;

namespace Logistics.AdminApp.Components.Pages.Subscription;

public partial class EditSubscription
{
    private SubscriptionDto _subscription = new();
    private IEnumerable<SubscriptionPlanDto>? _subscriptionPlans;
    private IEnumerable<TenantDto>? _tenants;
    
    
    #region Parameters

    [Parameter]
    public string? Id { get; set; }

    [Parameter] 
    public bool EditMode { get; set; } = true;

    #endregion
    

    #region Injectable services

    [Inject] 
    private NavigationManager Navigation { get; set; } = default!;

    #endregion
    

    protected override async Task OnInitializedAsync()
    {
        IsLoading = true;
        
        if (EditMode)
        {
            await FetchSubscriptionAsync();
        }
        
        await FetchSubscriptionPlansAsync();
        await FetchTenantsAsync();
        IsLoading = false;
    }

    private async Task FetchSubscriptionAsync()
    {
        var subscription = await CallApiAsync(api => api.GetSubscriptionAsync(Id!));

        if (subscription is not null)
        {
            _subscription = subscription;
        }
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

    private async Task SubmitAsync()
    {
        IsLoading = true;
        
        if (EditMode)
        {
            var success = await CallApiAsync(api => api.UpdateSubscriptionAsync(new UpdateSubscription
            {
                Id = _subscription.Id,
                PlanId = _subscription.Plan?.Id,
                TenantId = _subscription.Tenant?.Id
            }));

            if (success)
            {
                ShowNotification("The subscription data has been saved successfully");
            }
        }
        else
        {
            var success = await CallApiAsync(api => api.CreateSubscriptionAsync(new CreateSubscription
            {
                Status = _subscription.Status,
                PlanId = _subscription.Plan?.Id,
                TenantId = _subscription.Tenant?.Id
            }));

            if (success)
            {
                ShowNotification("A new subscription has been created successfully");
                ResetData();
            }
        }

        IsLoading = false;
    }

    private void ResetData()
    {
        _subscription = new SubscriptionDto();
    }
}
