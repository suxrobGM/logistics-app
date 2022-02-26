using Microsoft.AspNetCore.Components;

namespace Logistics.AdminApp.ViewModels.Pages.Truck;

public class EditTruckViewModel : PageViewModelBase
{

    public EditTruckViewModel(IApiClient apiClient)
        : base(apiClient)
    {
    }

    [Parameter]
    public string? Id { get; set; }
}
