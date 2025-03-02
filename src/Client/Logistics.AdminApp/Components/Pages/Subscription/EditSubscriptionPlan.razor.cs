using Logistics.Shared.Models;
using Microsoft.AspNetCore.Components;

namespace Logistics.AdminApp.Components.Pages.Subscription;

public partial class EditSubscriptionPlan : PageBase
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
        
        var subscriptionPlan = await CallApiAsync(api => api.GetSubscriptionPlanAsync(Id!));

        if (subscriptionPlan is not null)
        {
            _subscriptionPlan = subscriptionPlan;
        }
    }

    private async Task SubmitAsync()
    {
        if (EditMode)
        {
            var success = await CallApiAsync(api => api.UpdateSubscriptionPlanAsync(new UpdateSubscriptionPlan
            {
                Id = _subscriptionPlan.Id,
                Name = _subscriptionPlan.Name,
                Description = _subscriptionPlan.Description,
                Price = _subscriptionPlan.Price,
                HasTrial = _subscriptionPlan.HasTrial
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
                Price = _subscriptionPlan.Price,
                HasTrial = _subscriptionPlan.HasTrial
            }));

            if (success)
            {
                ShowNotification("A new subscription plan has been created successfully");
                ResetData();
            }
        }
    }

    private void ResetData()
    {
        _subscriptionPlan = new SubscriptionPlanDto();
    }
}
