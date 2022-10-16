using Logistics.Shared;

namespace Logistics.AdminApp.ViewModels.Pages;

public class ListTenantViewModel : PageBaseViewModel
{
    private readonly IApiClient _apiClient;

    public ListTenantViewModel(IApiClient apiClient)
    {
        _apiClient = apiClient;
        Tenants = Array.Empty<Tenant>();
        TenantsList = new PagedList<Tenant>(20, true, i => i.Id!);
        SearchInput = string.Empty;
    }

    public PagedList<Tenant> TenantsList { get; set; }
    public IEnumerable<Tenant> Tenants { get; set; }
    public string SearchInput { get; set; }

    private int _totalRecords;
    public int TotalRecords
    {
        get => _totalRecords;
        set => SetProperty(ref _totalRecords, value);
    }

    public override async Task OnInitializedAsync()
    {
        await base.OnInitializedAsync();
        await LoadPage(new PageEventArgs { Page = 1 });
    }

    public async Task SearchAsync()
    {
        await LoadPage(new PageEventArgs { Page = 1 }, SearchInput);
    }

    public async Task LoadPage(PageEventArgs e, string searchInput = "")
    {
        var result = await _apiClient.GetTenantsAsync(new SearchableQuery(searchInput, e.Page));
        var pagedList = result.Items;
        
        if (result.Success && pagedList != null)
        {
            TenantsList.AddRange(pagedList);
            TenantsList.TotalItems = result.ItemsCount;
            TotalRecords = result.ItemsCount;
            Tenants = TenantsList.GetPage(e.Page);
        }
    }
}
