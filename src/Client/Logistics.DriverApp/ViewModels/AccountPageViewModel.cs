using Logistics.DriverApp.Models;

namespace Logistics.DriverApp.ViewModels;

public class AccountPageViewModel : ViewModelBase
{
    private readonly IAuthService _authService;
    private readonly IApiClient _apiClient;
    private AccountEditForm _accountForm;

    public AccountPageViewModel(
        IAuthService authService, 
        IApiClient apiClient)
    {
        _authService = authService;
        _apiClient = apiClient;
        _accountForm = new AccountEditForm();
        AccountCenterUrl = $"{_authService.Options.Authority}/account/manage";
        UpdateCommand = new AsyncRelayCommand(UpdateAccountAsync, () => !IsBusy);
        IsBusyChanged += (s, e) => UpdateCommand.NotifyCanExecuteChanged();

        Task.Run(FetchUserAsync);
    }

    public IAsyncRelayCommand UpdateCommand { get; }
    public string AccountCenterUrl { get; }
    
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

        IsBusy = true;
        var result = await _apiClient.GetUserAsync(userId);

        if (result.Success)
        {
            var user = result.Value!;
            AccountForm.UserName = user.UserName;
            AccountForm.FirstName = user.FirstName;
            AccountForm.LastName = user.LastName;
            AccountForm.Email = user.Email;
            AccountForm.PhoneNumber = user.PhoneNumber;
        }

        IsBusy = false;
    }

    private async Task UpdateAccountAsync()
    {
        var userId = _authService.User?.Id;
        AccountForm.Validate();

        if (AccountForm.HasErrors)
        {
            var errors = string.Join('\n', AccountForm.GetErrors().Select(i => i.ErrorMessage));
            await PopupHelpers.ShowError(errors);
            return;
        }

        IsBusy = true;
        var result = await _apiClient.UpdateUserAsync(new UpdateUser()
        {
            Id = userId,
            FirstName = AccountForm.FirstName,
            LastName = AccountForm.LastName,
            PhoneNumber = AccountForm.PhoneNumber,
        });

        if (result.Success)
        {
            await PopupHelpers.ShowSuccess("Account details updated successfully");
        }

        IsBusy = false;
    }
}
