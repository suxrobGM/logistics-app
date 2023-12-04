using Logistics.Client.Models;
using Logistics.Shared.Models;
using Microsoft.AspNetCore.Components;

namespace Logistics.AdminApp.Pages.Subscription;

public partial class EditSubscription
{
    private SubscriptionDto _subscription = new();
    private SubscriptionPlanDto? _selectedSubscriptionPlan;
    private TenantDto? _selectedTenant;
    
    
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
        if (!EditMode)
        {
            return;
        }
        
        IsLoading = true;
        var subscription = await CallApiAsync(api => api.GetSubscriptionAsync(Id!));

        if (subscription is not null)
        {
            _subscription = subscription;
            _selectedSubscriptionPlan = subscription.Plan;
            _selectedTenant = subscription.Tenant;
        }

        IsLoading = false;
    }

    private async Task SubmitAsync()
    {
        IsLoading = true;
        
        if (EditMode)
        {
            var success = await CallApiAsync(api => api.UpdateSubscriptionAsync(new UpdateSubscription
            {
                Id = _subscription.Id,
                PlanId = _selectedSubscriptionPlan?.Id,
                TenantId = _selectedTenant?.Id
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
                PlanId = _selectedSubscriptionPlan?.Id,
                TenantId = _selectedTenant?.Id
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
