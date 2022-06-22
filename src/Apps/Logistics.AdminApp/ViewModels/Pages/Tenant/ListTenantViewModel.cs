using Logistics.WebApi.Client.Exceptions;

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
        IsBusy = true;
        await base.OnInitializedAsync();
        await LoadPage(new PageEventArgs { Page = 1 });
        IsBusy = false;
    }

    public async Task SearchAsync()
    {
        try
        {
            await LoadPage(new PageEventArgs { Page = 1 }, SearchInput);
        }
        catch (ApiException e)
        {
            Error = e.Message;
        }
        finally
        {
            IsBusy = false;
        }
    }

    public async Task LoadPage(PageEventArgs e, string searchInput = "")
    {
        var pagedList = await apiClient.GetTenantsAsync(searchInput, e.Page);

        if (pagedList.Items != null)
        {
            TenantsList.AddRange(pagedList.Items);
            TenantsList.TotalItems = pagedList.TotalItems;
            TotalRecords = pagedList.TotalItems;
            Tenants = TenantsList.GetPage(e.Page);
        }
    }
}
