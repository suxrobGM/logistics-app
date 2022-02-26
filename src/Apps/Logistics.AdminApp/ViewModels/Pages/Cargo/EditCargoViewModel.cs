using Microsoft.AspNetCore.Components;

namespace Logistics.AdminApp.ViewModels.Pages.Cargo;

public class EditCargoViewModel : PageViewModelBase
{

    public EditCargoViewModel(IApiClient apiClient)
        : base(apiClient)
    {
        
    }

    [Parameter]
    public string? Id { get; set; }
}
