using Logistics.Shared;
using Logistics.Shared.Models;
using Microsoft.AspNetCore.Components;
using Radzen;

namespace Logistics.AdminApp.Pages.Subscription;

public partial class ListSubscriptions : PageBase
{
    private IEnumerable<SubscriptionDto>? _subscriptions;
    private int _totalRecords = 10;
    
    
    #region Injectable services

    [Inject]
    private NavigationManager Navigation { get; set; } = default!;

    #endregion
    
    
    private async void LoadData(LoadDataArgs e)
    {
        var page = (e.Skip ?? 0) + 1;
        var pageSize = e.Top ?? 10;
        var pagedData = await CallApiAsync(api => api.GetSubscriptionsAsync(new PagedQuery(page, pageSize)));
        _subscriptions = pagedData?.Items;
        _totalRecords = pagedData?.TotalItems ?? 0;
        StateHasChanged();
    }
}
