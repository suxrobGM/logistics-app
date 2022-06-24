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
        await LoadPage(new PageEventArgs { Page = 1 });
    }

    public async Task LoadPage(PageEventArgs e)
    {
        var result = await CallApi(i => i.GetEmployeesAsync(page: e.Page, pageSize: 20));

        if (!result.Success)
            return;
        
        var pagedList = result.Value;

        if (pagedList?.Items != null)
        {
            UsersList.AddRange(pagedList.Items);
            UsersList.TotalItems = pagedList.TotalItems;
            TotalRecords = pagedList.TotalItems;
            Users = UsersList.GetPage(e.Page);
        }
    }
}
