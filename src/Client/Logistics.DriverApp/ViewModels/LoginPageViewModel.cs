using CommunityToolkit.Mvvm.Messaging;
using Logistics.DriverApp.Messages;
using Logistics.DriverApp.Services;
using Logistics.DriverApp.Services.Authentication;

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
        IsLoadingChanged += HandleIsLoadingChanged;
    }

    
    #region Commands

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
            
            if (!string.IsNullOrEmpty(tenantId))
            {
                _apiClient.TenantId = tenantId;
                await _tenantService.SaveTenantIdAsync(tenantId);
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
    }
}
