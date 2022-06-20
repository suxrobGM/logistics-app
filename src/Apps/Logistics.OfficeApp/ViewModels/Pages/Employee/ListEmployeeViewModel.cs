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
        IsBusy = true;
        await LoadPage(new PageEventArgs { Page = 1 });
        IsBusy = false;
    }

    public async Task LoadPage(PageEventArgs e)
    {
        var pagedList = await FetchUsersAsync(e.Page);

        if (pagedList.Items != null)
        {
            UsersList.AddRange(pagedList.Items);
            UsersList.TotalItems = pagedList.TotalItems;
            TotalRecords = pagedList.TotalItems;
            Users = UsersList.GetPage(e.Page);
        }
    }

    private Task<PagedDataResult<EmployeeDto>> FetchUsersAsync(int page = 1)
    {
        return Task.Run(async () => await apiClient.GetEmployeesAsync(page: page, pageSize: 20));
    }
}
