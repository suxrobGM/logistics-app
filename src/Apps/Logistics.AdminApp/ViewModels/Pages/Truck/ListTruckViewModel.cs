namespace Logistics.AdminApp.ViewModels.Pages.Truck;

public class ListTruckViewModel : ViewModelBase
{
    private readonly IApiClient apiClient;

    public ListTruckViewModel(IApiClient apiClient)
    {
        this.apiClient = apiClient;
    }
}
