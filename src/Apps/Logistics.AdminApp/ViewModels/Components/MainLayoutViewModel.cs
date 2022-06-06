using Logistics.AdminApp.Services;

namespace Logistics.AdminApp.ViewModels.Components;

public class MainLayoutViewModel : ViewModelBase
{
    private readonly AuthenticationStateService _authStateService;

    public MainLayoutViewModel(AuthenticationStateService authStateService)
    {
        _authStateService = authStateService;
    }

    public override async Task OnInitializedAsync()
    {
        await _authStateService.ReevaluateAuthenticationStateAsync();
    }
}
