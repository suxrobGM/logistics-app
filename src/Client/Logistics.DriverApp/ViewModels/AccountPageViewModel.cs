using Logistics.DriverApp.Models;

namespace Logistics.DriverApp.ViewModels;

public class AccountPageViewModel : ViewModelBase
{
    private readonly IAuthService _authService;
    private readonly IApiClient _apiClient;

    public AccountPageViewModel(
        IAuthService authService, 
        IApiClient apiClient)
    {
        _authService = authService;
        _apiClient = apiClient;
        _accountForm = new AccountEditForm();
        UpdateCommand = new AsyncRelayCommand(UpdateAccountAsync);
    }

    public IAsyncRelayCommand UpdateCommand { get; }

    private AccountEditForm _accountForm;
    public AccountEditForm AccountForm 
    {
        get => _accountForm;
        set => SetProperty(ref _accountForm, value);
    }

    private async Task FetchUserAsync()
    {
        var userId = _authService.User?.Id;

        if (string.IsNullOrEmpty(userId))
            return;

        
    }

    private Task UpdateAccountAsync()
    {
        return Task.CompletedTask;
    }
}
