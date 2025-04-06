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
    private NavigationManager Navigation { get; set; } = null!;

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

    private async Task SubmitAsync(SubscriptionPlanDto model)
    {
        if (EditMode)
        {
            var success = await CallApiAsync(api => api.UpdateSubscriptionPlanAsync(new UpdateSubscriptionPlan
            {
                Id = model.Id,
                Name = model.Name,
                Description = model.Description,
                Price = model.Price,
                TrialPeriod = model.TrialPeriod,
                Interval = model.Interval,
                IntervalCount = model.IntervalCount
            }));

            if (success)
            {
                ShowNotification("The subscription plan has been saved successfully");
            }
        }
        else
        {
            var success = await CallApiAsync(api => api.CreateSubscriptionPlanAsync(new CreateSubscriptionPlan
            {
                Name = model.Name,
                Description = model.Description,
                Price = model.Price,
                TrialPeriod = model.TrialPeriod,
                Interval = model.Interval,
                IntervalCount = model.IntervalCount
            }));

            if (success)
            {
                ShowNotification("A new subscription plan has been created successfully");
                ResetData();
                Navigation.NavigateTo("/subscription-plans");
            }
        }
    }

    private void ResetData()
    {
        _subscriptionPlan = new SubscriptionPlanDto();
    }
}
