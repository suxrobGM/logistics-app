using Microsoft.AspNetCore.Components;

namespace Logistics.AdminApp.ViewModels.Pages.Cargo;

public class EditCargoViewModel : ViewModelBase
{
    private readonly IApiClient apiClient;

    public EditCargoViewModel(IApiClient apiClient)
    {
        this.apiClient = apiClient;
    }

    [Parameter]
    public string? Id { get; set; }
}
