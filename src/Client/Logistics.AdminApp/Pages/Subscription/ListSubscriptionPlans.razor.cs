using Logistics.Shared;
using Logistics.Shared.Models;
using Microsoft.AspNetCore.Components;
using Radzen;

namespace Logistics.AdminApp.Pages.Subscription;

public partial class ListSubscriptionPlans : PageBase
{
    private IEnumerable<SubscriptionPlanDto>? _subscriptionPlans;
    private int _totalRecords = 10;
    
    
    #region Injectable services

    [Inject]
    private NavigationManager Navigation { get; set; } = default!;

    #endregion
    
    private async void LoadTenants(LoadDataArgs e)
    {
        var page = (e.Skip ?? 0) + 1;
        var pageSize = e.Top ?? 10;
        var pagedData = await CallApiAsync(api => api.GetSubscriptionPlansAsync(new PagedQuery(page, pageSize)));
        _subscriptionPlans = pagedData?.Items;
        _totalRecords = pagedData?.TotalItems ?? 0;
        StateHasChanged();
    }

    private void OpenEditPage(SubscriptionPlanDto subscriptionPlan)
    {
        Navigation.NavigateTo($"subscription-plans/{subscriptionPlan.Id}");
    }
}
