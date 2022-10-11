using Logistics.DriverApp.Authentication;

namespace Logistics.DriverApp.ViewModels;

public class LoginPageViewModel : ViewModelBase
{
    private readonly IAuthService _authService;

    public LoginPageViewModel(IAuthService authService)
    {
        _authService = authService;
        SignInCommand = new AsyncRelayCommand(LoginAsync);
    }

    public IAsyncRelayCommand SignInCommand { get; }

    public async Task LoginAsync()
    {
        await _authService.LoginAsync();
    }
}
