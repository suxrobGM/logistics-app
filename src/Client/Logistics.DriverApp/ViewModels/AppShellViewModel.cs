using Logistics.DriverApp.Services;
using Logistics.DriverApp.Services.Authentication;

namespace Logistics.DriverApp.ViewModels;

public class AppShellViewModel : ViewModelBase
{
    private readonly IAuthService _authService;
    private readonly IApiClient _apiClient;

    public AppShellViewModel(IAuthService authService, IApiClient apiClient)
    {
        _authService = authService;
        _apiClient = apiClient;
        _apiClient.OnErrorResponse += async (s, e) => await HandleApiErrors(e);
        SignOutCommand = new AsyncRelayCommand(SignOutAsync);
    }

    public IAsyncRelayCommand SignOutCommand { get; }


    #region Bindable properties

    public bool DashboardPageVisible
    {
        get =>!string.IsNullOrEmpty(TenantService.TenantId);
    }

    #endregion


    public async Task SignOutAsync()
    {
        IsBusy = true;
        await _authService.LogoutAsync();
        await Shell.Current.GoToAsync("//LoginPage", true);
        IsBusy = false;
    }

    private static Task HandleApiErrors(string error)
    {
        return MainThread.InvokeOnMainThreadAsync(() => PopupHelpers.ShowError(error));
    }
}
