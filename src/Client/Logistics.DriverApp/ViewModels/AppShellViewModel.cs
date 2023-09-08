using CommunityToolkit.Mvvm.Messaging;
using Logistics.DriverApp.Messages;
using Logistics.DriverApp.Services.Authentication;

namespace Logistics.DriverApp.ViewModels;

public class AppShellViewModel : BaseViewModel
{
    private readonly IAuthService _authService;
    private readonly IApiClient _apiClient;

    public AppShellViewModel(IAuthService authService, IApiClient apiClient)
    {
        _authService = authService;
        _apiClient = apiClient;
        _apiClient.OnErrorResponse += async (s, e) => await HandleApiErrors(e);
        SignOutCommand = new AsyncRelayCommand(SignOutAsync);
        ActiveLoadsPageVisible = true;
        Messenger.Register<TenantIdChangedMessage>(this, (s, m) =>
        {
            ActiveLoadsPageVisible = !string.IsNullOrEmpty(m.Value);
        });
    }

    public IAsyncRelayCommand SignOutCommand { get; }


    #region Bindable properties

    private bool _activeLoadsPageVisible;
    public bool ActiveLoadsPageVisible
    {
        get => _activeLoadsPageVisible;
        set => SetProperty(ref _activeLoadsPageVisible, value);
    }

    #endregion

    private async Task SignOutAsync()
    {
        IsLoading = true;
        await _authService.LogoutAsync();
        await Shell.Current.GoToAsync("//LoginPage", true);
        IsLoading = false;
    }

    private static Task HandleApiErrors(string error)
    {
        return MainThread.InvokeOnMainThreadAsync(() => PopupHelpers.ShowErrorAsync(error));
    }
}
