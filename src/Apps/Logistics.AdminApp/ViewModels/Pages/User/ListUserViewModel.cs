namespace Logistics.AdminApp.ViewModels.Pages.User;

public class ListUserViewModel : ViewModelBase
{
    private readonly IApiClient apiClient;

    public ListUserViewModel(IApiClient apiClient)
    {
        this.apiClient = apiClient;
    }
}
