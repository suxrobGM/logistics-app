namespace Logistics.DriverApp.ViewModels;

public class LoginPageViewModel : ViewModelBase
{
    public LoginPageViewModel()
    {
        SignInCommand = new AsyncRelayCommand(LoginAsync);
    }

    public IAsyncRelayCommand SignInCommand { get; }

    public Task LoginAsync()
    {
        return Task.CompletedTask;
    }
}
