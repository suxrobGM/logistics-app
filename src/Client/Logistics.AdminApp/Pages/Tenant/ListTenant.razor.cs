using Logistics.Shared;
using Microsoft.AspNetCore.Authorization;

namespace Logistics.AdminApp.Pages.Tenant;

[Authorize(Policy = Permissions.Tenant.View)]
public partial class ListTenant : PageBase
{
    private readonly PagedList<Client.Models.Tenant> _tenantsPagedList = new(20, true, i => i.Id!);
    private IEnumerable<Client.Models.Tenant> _tenants = Array.Empty<Client.Models.Tenant>();
    private string _searchInput = string.Empty;
    private int _totalRecords;
    
    protected override async Task OnInitializedAsync()
    {
        await LoadPage(new PageEventArgs { Page = 1 });
    }

    private Task SearchAsync()
    {
        Console.WriteLine("Search");
        return LoadPage(new PageEventArgs { Page = 1 }, _searchInput);
    }

    private async Task LoadPage(PageEventArgs e, string searchInput = "")
    {
        var result = await ApiClient.GetTenantsAsync(new SearchableQuery(searchInput, e.Page));
        var pagedList = result.Items;
        
        if (result.Success && pagedList != null)
        {
            _tenantsPagedList.AddRange(pagedList);
            _tenantsPagedList.TotalItems = result.ItemsCount;
            _totalRecords = result.ItemsCount;
            _tenants = _tenantsPagedList.GetPage(e.Page);
            StateHasChanged();
        }
    }
}