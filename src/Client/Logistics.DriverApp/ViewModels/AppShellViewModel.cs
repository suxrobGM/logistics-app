using CommunityToolkit.Mvvm.Messaging;
using Logistics.DriverApp.Messages;
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
        Messenger.Register<TenantIdChangedMessage>(this, (s, m) =>
        {
            DashboardPageVisible = !string.IsNullOrEmpty(m.Value);
        });
    }

    public IAsyncRelayCommand SignOutCommand { get; }


    #region Bindable properties

    private bool _dashboardPageVisible;
    public bool DashboardPageVisible
    {
        get => _dashboardPageVisible;
        set => SetProperty(ref _dashboardPageVisible, value);
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
