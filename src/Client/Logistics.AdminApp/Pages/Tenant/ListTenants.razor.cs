using Logistics.Shared.Models;
using Logistics.Shared;
using Logistics.Shared.Policies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Radzen;

namespace Logistics.AdminApp.Pages.Tenant;

[Authorize(Policy = Permissions.Tenants.View)]
public partial class ListTenants : PageBase
{
    private IEnumerable<TenantDto>? _tenants;
    private int _totalRecords = 10;
    
    #region Injectable services

    [Inject] 
    private NavigationManager Navigation { get; set; } = default!;

    [Inject]
    private AuthenticationStateProvider AuthStateProvider { get; set; } = default!;

    #endregion

    protected override async Task OnInitializedAsync()
    {
        var authState = await AuthStateProvider.GetAuthenticationStateAsync();
        var claims = authState.User.Claims.ToArray();
    }

    private async void LoadTenants(LoadDataArgs e)
    {
        var page = (e.Skip ?? 0) + 1;
        var pageSize = e.Top ?? 10;
        var pagedData = await CallApiAsync(api => api.GetTenantsAsync(new SearchableQuery(null, page, pageSize)));
        _tenants = pagedData?.Items;
        _totalRecords = pagedData?.TotalItems ?? 0;
        StateHasChanged();
    }

    private void OpenEditPage(TenantDto tenant)
    {
        Navigation.NavigateTo($"tenants/{tenant.Id}");
    }
}
