namespace Logistics.AdminApp.ViewModels.Pages.User;

public class ListUserViewModel : PageViewModelBase
{
    public ListUserViewModel(IApiClient apiClient)
        : base(apiClient)
    {
        _users = new List<UserDto>();
    }


    #region Binding properties

    private IList<UserDto> _users;
    public IList<UserDto> Users
    {
        get => _users;
        set => SetProperty(ref _users, value);
    }

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
        var pagedList = await FetchUsersAsync();

        if (pagedList != null && pagedList.Items != null)
        {
            Users = pagedList.Items;
            TotalRecords = pagedList.TotalItems;
        }

        IsBusy = false;
    }

    private Task<PagedDataResult<UserDto>> FetchUsersAsync(int page = 1)
    {
        return Task.Run(async () =>
        {
            return await apiClient.GetUsersAsync(page);
        });
    }
}
