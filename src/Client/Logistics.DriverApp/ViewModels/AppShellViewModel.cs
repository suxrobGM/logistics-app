namespace Logistics.DriverApp.ViewModels;

public class AppShellViewModel : ViewModelBase
{
    private readonly IAuthService _authService;

    public AppShellViewModel(IAuthService authService)
    {
        _authService = authService;
        SignOutCommand = new AsyncRelayCommand(SignOutAsync);
    }

    public IAsyncRelayCommand SignOutCommand { get; }

    public async Task SignOutAsync()
    {
        IsBusy = true;
        var result = await _authService.LogoutAsync();
        await Shell.Current.GoToAsync("//LoginPage", true);
        IsBusy = false;
    }
}
