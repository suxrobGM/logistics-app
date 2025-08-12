using Logistics.AdminApp.Extensions;
using Logistics.Shared.Models;

using Microsoft.AspNetCore.Components;

using Radzen;

namespace Logistics.AdminApp.Components.Pages.Tenant;

public partial class ListTenants : PageBase
{
    private IEnumerable<TenantDto>? _tenants;
    private int _totalRecords = 10;

    #region Injectable services

    [Inject]
    private NavigationManager Navigation { get; set; } = default!;

    #endregion

    private async void LoadTenants(LoadDataArgs e)
    {
        var orderBy = e.GetOrderBy();
        var page = e.GetPageNumber();
        var pageSize = e.GetPageSize();
        var pagedData = await CallApiAsync(api => api.GetTenantsAsync(new SearchableQuery(null, orderBy, page, pageSize)));
        _tenants = pagedData?.Items;
        _totalRecords = pagedData?.TotalItems ?? 0;
        StateHasChanged();
    }
}
