namespace Logistics.AdminApp.ViewModels.Pages.Tenant;

public class ListTenantViewModel : PageViewModelBase
{
    public ListTenantViewModel(IApiClient apiClient) : base(apiClient)
    {
        Tenants = Array.Empty<TenantDto>();
        TenantsList = new PagedList<TenantDto>(20, true, i => i.Id!);
        SearchInput = string.Empty;
    }

    public PagedList<TenantDto> TenantsList { get; set; }
    public IEnumerable<TenantDto> Tenants { get; set; }
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
        var pagedListResult = await CallApi(i => i.GetTenantsAsync(searchInput, e.Page));
        var pagedList = pagedListResult.Value;
        
        if (pagedListResult.Success && pagedList?.Items != null)
        {
            TenantsList.AddRange(pagedList.Items);
            TenantsList.TotalItems = pagedList.ItemsCount;
            TotalRecords = pagedList.ItemsCount;
            Tenants = TenantsList.GetPage(e.Page);
        }
    }
}
