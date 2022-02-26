using Microsoft.AspNetCore.Components;

namespace Logistics.AdminApp.ViewModels.Pages.User;

public class EditUserViewModel : PageViewModelBase
{

    public EditUserViewModel(IApiClient apiClient)
        : base(apiClient)
    {
        
    }

    [Parameter]
    public string? Id { get; set; }
}
