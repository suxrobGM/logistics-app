using Logistics.DriverApp.Models;
using Logistics.DriverApp.Services.Authentication;
using Logistics.Models;

namespace Logistics.DriverApp.ViewModels;

public class AccountPageViewModel : ViewModelBase
{
    private readonly IAuthService _authService;
    private readonly IApiClient _apiClient;
    private AccountEditForm _accountForm;
    private UserDto? _user;

    public AccountPageViewModel(
        IAuthService authService, 
        IApiClient apiClient)
    {
        _authService = authService;
        _apiClient = apiClient;
        _accountForm = new AccountEditForm();
        AccountCenterUrl = $"{_authService.Options.Authority}/account/manage";
        SaveCommand = new AsyncRelayCommand(UpdateAccountAsync, () => !IsLoading);
        IsLoadingChanged += (s, e) => SaveCommand.NotifyCanExecuteChanged();
    }

    public IAsyncRelayCommand SaveCommand { get; }
    public string AccountCenterUrl { get; }
    
    public AccountEditForm AccountForm 
    {
        get => _accountForm;
        set => SetProperty(ref _accountForm, value);
    }

    protected override async void OnActivated()
    {
        await FetchUserAsync();
    }

    private async Task FetchUserAsync()
    {
        if (_user != null)
            return;
        
        var userId = _authService.User?.Id;

        if (string.IsNullOrEmpty(userId))
            return;
        
        IsLoading = true;
        var result = await _apiClient.GetUserAsync(userId);

        if (result.Success)
        {
            _user = result.Value!;
            AccountForm.UserName = _user.UserName;
            AccountForm.FirstName = _user.FirstName;
            AccountForm.LastName = _user.LastName;
            AccountForm.Email = _user.Email;
            AccountForm.PhoneNumber = _user.PhoneNumber;
        }
        
        IsLoading = false;
    }

    private async Task UpdateAccountAsync()
    {
        var userId = _authService.User?.Id;
        AccountForm.Validate();

        if (AccountForm.HasErrors)
        {
            var errors = string.Join('\n', AccountForm.GetErrors().Select(i => i.ErrorMessage));
            await PopupHelpers.ShowErrorAsync(errors);
            return;
        }

        IsLoading = true;
        var result = await _apiClient.UpdateUserAsync(new UpdateUser()
        {
            Id = userId,
            FirstName = AccountForm.FirstName,
            LastName = AccountForm.LastName,
            PhoneNumber = AccountForm.PhoneNumber,
        });

        if (result.Success)
        {
            await PopupHelpers.ShowSuccessAsync("Account details updated successfully");
        }

        IsLoading = false;
    }
}
