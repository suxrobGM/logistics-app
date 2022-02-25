using Microsoft.AspNetCore.Components;

namespace Logistics.AdminApp.ViewModels.Pages.User;

public class EditUserViewModel : ViewModelBase
{
    private readonly IApiClient apiClient;

    public EditUserViewModel(IApiClient apiClient)
    {
        this.apiClient = apiClient;
    }

    [Parameter]
    public string? Id { get; set; }
}
