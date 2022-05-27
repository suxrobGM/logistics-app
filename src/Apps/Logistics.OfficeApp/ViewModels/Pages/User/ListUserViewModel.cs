namespace Logistics.OfficeApp.ViewModels.Pages.User;

public class ListUserViewModel : PageViewModelBase
{
    public ListUserViewModel(IApiClient apiClient)
        : base(apiClient)
    {
        Users = Array.Empty<UserDto>();
        UsersList = new PagedList<UserDto>(20, true, i => i.Id!);
    }


    #region Binding properties

    public PagedList<UserDto> UsersList { get; set; }
    public IEnumerable<UserDto> Users { get; set; }

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
        await LoadPage(new() { Page = 1 });
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

    private Task<PagedDataResult<UserDto>> FetchUsersAsync(int page = 1)
    {
        return Task.Run(async () =>
        {
            return await apiClient.GetUsersAsync(page: page, pageSize: 20);
        });
    }
}
