namespace Logistics.AdminApp.ViewModels.Pages.Cargo;

public class ListCargoViewModel : ViewModelBase
{
    private readonly IApiClient apiClient;

    public ListCargoViewModel(IApiClient apiClient)
    {
        this.apiClient = apiClient;
    }
}
