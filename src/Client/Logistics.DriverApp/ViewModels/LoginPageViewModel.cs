namespace Logistics.DriverApp.ViewModels;

public class LoginPageViewModel : ViewModelBase
{
    private readonly IAuthService _authService;
    private readonly IApiClient _apiClient;

    public LoginPageViewModel(
        IAuthService authService,
        IApiClient apiClient)
    {
        _authService = authService;
        _apiClient = apiClient;
        SignInCommand = new AsyncRelayCommand(LoginAsync, () => !IsBusy);
        OpenSignUpCommand = new AsyncRelayCommand(OpenSignUpUrl, () => !IsBusy);
        IsBusyChanged += HandleIsBusyChanged;
    }

    public IAsyncRelayCommand SignInCommand { get; }
    public IAsyncRelayCommand OpenSignUpCommand { get; }

    private async Task LoginAsync()
    {
        IsBusy = true;
        try
        {
            var result = await _authService.LoginAsync();
            
            if (result.IsError)
                return;

            _apiClient.AccessToken = result.AccessToken;
            string? tenantId = await SecureStorage.Default.GetAsync(StorageKeys.TenantId);

            if (!string.IsNullOrEmpty(tenantId))
            {
                _apiClient.TenantId = tenantId;
                await Shell.Current.GoToAsync("//DashboardPage");
            }
            else
            {
                await Shell.Current.GoToAsync("//ChangeOrganizationPage");
            }
        }
        catch (Exception ex)
        {
            await PopupHelpers.ShowError(ex.Message);
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task OpenSignUpUrl()
    {
        var url = $"{_authService.Options.Authority}/account/register";
        await Launcher.OpenAsync(url);
    }

    private void HandleIsBusyChanged(object? sender, bool value)
    {
        OpenSignUpCommand.NotifyCanExecuteChanged();
        SignInCommand.NotifyCanExecuteChanged();
    }
}
