namespace Logistics.OfficeApp.ViewModels.Pages.Employee;

public class ListUserViewModel : PageViewModelBase
{
    public ListUserViewModel(IApiClient apiClient)
        : base(apiClient)
    {
        Users = Array.Empty<EmployeeDto>();
        UsersList = new PagedList<EmployeeDto>(20, true, i => i.Id!);
    }


    #region Binding properties

    public PagedList<EmployeeDto> UsersList { get; set; }
    public IEnumerable<EmployeeDto> Users { get; set; }

    private int _totalRecords;
    public int TotalRecords
    {
        get => _totalRecords;
        set => SetProperty(ref _totalRecords, value);
    }

    #endregion


    public override async Task OnInitializedAsync()
    {
        await base.OnInitializedAsync();
        
        try
        {
            IsBusy = true;
            await LoadPage(new PageEventArgs { Page = 1 });
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

    public async Task LoadPage(PageEventArgs e)
    {
        var pagedList = await apiClient.GetEmployeesAsync(page: e.Page, pageSize: 20);

        if (pagedList.Items != null)
        {
            UsersList.AddRange(pagedList.Items);
            UsersList.TotalItems = pagedList.TotalItems;
            TotalRecords = pagedList.TotalItems;
            Users = UsersList.GetPage(e.Page);
        }
    }
}
