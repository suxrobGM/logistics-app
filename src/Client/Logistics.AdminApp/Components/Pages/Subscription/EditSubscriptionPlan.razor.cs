using Logistics.Client.Models;
using Logistics.Shared.Models;
using Microsoft.AspNetCore.Components;

namespace Logistics.AdminApp.Components.Pages.Subscription;

public partial class EditSubscriptionPlan
{
    private SubscriptionPlanDto _subscriptionPlan = new();
    
    
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
        var subscriptionPlan = await CallApiAsync(api => api.GetSubscriptionPlanAsync(Id!));

        if (subscriptionPlan is not null)
        {
            _subscriptionPlan = subscriptionPlan;
        }

        IsLoading = false;
    }

    private async Task SubmitAsync()
    {
        IsLoading = true;
        
        if (EditMode)
        {
            var success = await CallApiAsync(api => api.UpdateSubscriptionPlanAsync(new UpdateSubscriptionPlan
            {
                Id = _subscriptionPlan.Id,
                Name = _subscriptionPlan.Name,
                Description = _subscriptionPlan.Description,
                Price = _subscriptionPlan.Price,
            }));

            if (!success)
            {
                return;
            }
            
            ShowNotification("The subscription plan has been saved successfully");
        }
        else
        {
            var success = await CallApiAsync(api => api.CreateSubscriptionPlanAsync(new CreateSubscriptionPlan
            {
                Name = _subscriptionPlan.Name,
                Description = _subscriptionPlan.Description,
                Price = _subscriptionPlan.Price
            }));

            if (success)
            {
                ShowNotification("A new subscription plan has been created successfully");
                ResetData();
            }
        }

        IsLoading = false;
    }

    private void ResetData()
    {
        _subscriptionPlan = new SubscriptionPlanDto();
    }
}
