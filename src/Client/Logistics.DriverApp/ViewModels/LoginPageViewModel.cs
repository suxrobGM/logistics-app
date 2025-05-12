using CommunityToolkit.Mvvm.Messaging;
using Logistics.DriverApp.Messages;
using Logistics.DriverApp.Services;
using Logistics.DriverApp.Services.Authentication;
using Plugin.Fingerprint;
using Plugin.Fingerprint.Abstractions;

namespace Logistics.DriverApp.ViewModels;

public class LoginPageViewModel : BaseViewModel
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
        BiometricLoginCommand = new AsyncRelayCommand(BiometricLoginAsync, () => !IsLoading);
        IsLoadingChanged += HandleIsLoadingChanged;
    }

    
    #region Commands

    public IAsyncRelayCommand BiometricLoginCommand { get; }
    
    public IAsyncRelayCommand SignInCommand { get; }
    public IAsyncRelayCommand OpenSignUpCommand { get; }

    #endregion
    

    protected override async Task OnInitializedAsync()
    {
        var canAutoLogin = await _authService.CanAutoLoginAsync();
    
        if (canAutoLogin)
        {
            await LoginAsync(); // try auto login
        }
    }
    
    private async Task BiometricLoginAsync()
    {
        IsLoading = true;
        try
        {
            var canAutoLogin = await _authService.CanAutoLoginAsync();
            if (!canAutoLogin)
            {
                await PopupHelpers.ShowErrorAsync("Please login first to enable biometrics");
                return;
            }

            var availability = await CrossFingerprint.Current.GetAvailabilityAsync();
            if (availability != FingerprintAvailability.Available)
            {
                await PopupHelpers.ShowErrorAsync("Biometrics not available");
                return;
            }

            var request = new AuthenticationRequestConfiguration(
                "Biometric Login",
                "Authenticate to access your account");
            
            var result = await CrossFingerprint.Current.AuthenticateAsync(request);
            
            if (result.Authenticated)
            {
                await LoginAsync(); // Proceed with existing login flow
            }
            else
            {
                await PopupHelpers.ShowErrorAsync("Biometric authentication failed");
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
            var tenantId = await _tenantService.GetTenantIdFromCacheAsync() ?? _authService.User?.TenantId;

            Messenger.Send(new UserLoggedInMessage(_authService.User!));
            
            if (tenantId.HasValue)
            {
                _apiClient.TenantId = tenantId;
                await _tenantService.SaveTenantIdAsync(tenantId.Value);
                await Shell.Current.GoToAsync("//DashboardPage");
            }
            else
            {
                await PopupHelpers.ShowErrorAsync("You have not joined any company yet. Please contact your company administrator to get an invite.");
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
        BiometricLoginCommand.NotifyCanExecuteChanged();
    }
}
