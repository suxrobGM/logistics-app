using Microsoft.AspNetCore.Components;

namespace Logistics.AdminApp.ViewModels.Pages.Truck;

public class EditTruckViewModel : ViewModelBase
{
    private readonly IApiClient apiClient;

    public EditTruckViewModel(IApiClient apiClient)
    {
        this.apiClient = apiClient;
    }

    [Parameter]
    public string? Id { get; set; }
}
