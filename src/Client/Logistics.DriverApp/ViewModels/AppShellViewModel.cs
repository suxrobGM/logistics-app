using CommunityToolkit.Mvvm.Messaging;
using Logistics.DriverApp.Messages;
using Logistics.DriverApp.Services;
using Logistics.DriverApp.Services.Authentication;

namespace Logistics.DriverApp.ViewModels;

public class AppShellViewModel : BaseViewModel
{
    private readonly IAuthService _authService;
    private readonly IApiClient _apiClient;
    private readonly ITenantService _tenantService;

    public AppShellViewModel(
        IAuthService authService,
        ITenantService tenantService,
        IApiClient apiClient)
    {
        _authService = authService;
        _apiClient = apiClient;
        _tenantService = tenantService;
        _apiClient.OnErrorResponse += async (s, e) => await HandleApiErrors(e);
        SignOutCommand = new AsyncRelayCommand(SignOutAsync);
        ChangeOrganizationPageVisible = true;
        Messenger.Register<SuccessfullyLoggedMessage>(this, (_, m) =>
        {
            ChangeOrganizationPageVisible = m.Value.TenantIds.Count > 1;
        });
    }

    public IAsyncRelayCommand SignOutCommand { get; }


    #region Bindable properties
    
    private bool _changeOrganizationPageVisible;
    public bool ChangeOrganizationPageVisible
    {
        get => _changeOrganizationPageVisible;
        set => SetProperty(ref _changeOrganizationPageVisible, value);
    }

    #endregion

    private async Task SignOutAsync()
    {
        IsLoading = true;
        _tenantService.ClearCache();
        await _authService.LogoutAsync();
        await Shell.Current.GoToAsync("//LoginPage", true);
        IsLoading = false;
    }

    private static Task HandleApiErrors(string error)
    {
        return MainThread.InvokeOnMainThreadAsync(() => PopupHelpers.ShowErrorAsync(error));
    }
}
