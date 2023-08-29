using Logistics.DriverApp.Services;
using Logistics.DriverApp.Services.Authentication;

namespace Logistics.DriverApp.ViewModels;

public class LoginPageViewModel : ViewModelBase
{
    private readonly IAuthService _authService;
    private readonly IApiClient _apiClient;
    private readonly ITenantService _tenantService;

    public LoginPageViewModel(
        IAuthService authService,
        IApiClient apiClient,
        ITenantService tenantService)
    {
        _authService = authService;
        _apiClient = apiClient;
        _tenantService = tenantService;
        SignInCommand = new AsyncRelayCommand(LoginAsync, () => !IsLoading);
        OpenSignUpCommand = new AsyncRelayCommand(OpenSignUpUrl, () => !IsLoading);
        IsLoadingChanged += HandleIsLoadingChanged;
    }

    public IAsyncRelayCommand SignInCommand { get; }
    public IAsyncRelayCommand OpenSignUpCommand { get; }

    private async Task LoginAsync()
    {
        IsLoading = true;
        try
        {
            var result = await _authService.LoginAsync();
            
            if (result.IsError)
            {
                await PopupHelpers.ShowErrorAsync(result.ErrorDescription);
                return;
            }

            _apiClient.AccessToken = result.AccessToken;
            var tenantId = await _tenantService.GetTenantIdFromCacheAsync();

            if (!string.IsNullOrEmpty(tenantId))
            {
                _apiClient.TenantId = tenantId;
                await _tenantService.SaveTenantIdAsync(tenantId);
                await Shell.Current.GoToAsync("//DashboardPage");
            }
            else
            {
                await Shell.Current.GoToAsync("//ChangeOrganizationPage");
            }
        }
        catch (Exception ex)
        {
            await PopupHelpers.ShowErrorAsync(ex.Message);
        }
        finally
        {
            IsLoading = false;
        }
    }

    private async Task OpenSignUpUrl()
    {
        var url = $"{_authService.Options.Authority}/account/register";
        await Launcher.OpenAsync(url);
    }

    private void HandleIsLoadingChanged(object? sender, bool value)
    {
        OpenSignUpCommand.NotifyCanExecuteChanged();
        SignInCommand.NotifyCanExecuteChanged();
    }
}
