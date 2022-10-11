using Logistics.DriverApp.Authentication;

namespace Logistics.DriverApp.ViewModels;

public class LoginPageViewModel : ViewModelBase
{
    private readonly IAuthService _authService;

    public LoginPageViewModel(IAuthService authService)
    {
        _authService = authService;
        SignInCommand = new AsyncRelayCommand(LoginAsync, () => !IsBusy);
        OpenSignUpCommand = new AsyncRelayCommand(OpenSignUpUrl, () => !IsBusy);
        IsBusyChanged += (s, e) => SignInCommand.NotifyCanExecuteChanged();
    }

    public IAsyncRelayCommand SignInCommand { get; }
    public IAsyncRelayCommand OpenSignUpCommand { get; }

    public async Task LoginAsync()
    {
        IsBusy = true;
        var result = await _authService.LoginAsync();

        if (!result.IsError)
        {
            await Shell.Current.GoToAsync("//MainPage", true);
        }
        IsBusy = false;
    }

    public async Task OpenSignUpUrl()
    {
        var url = $"{_authService.Options.Authority}/account/register";
        await Launcher.OpenAsync(url);
    }
}
