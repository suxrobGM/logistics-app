using Logistics.DriverApp.Models;
using Logistics.DriverApp.Services.Authentication;

namespace Logistics.DriverApp.ViewModels;

public class AccountPageViewModel : BaseViewModel
{
    private readonly IAuthService _authService;
    private readonly IApiClient _apiClient;

    public AccountPageViewModel(
        IAuthService authService, 
        IApiClient apiClient)
    {
        _authService = authService;
        _apiClient = apiClient;
        _accountInfo = new AccountInfo();
        // AccountCenterUrl = $"{_authService.Options.Authority}/account/manage";
        SaveCommand = new AsyncRelayCommand(UpdateAccountAsync, () => !IsLoading);
        IsLoadingChanged += (s, e) => SaveCommand.NotifyCanExecuteChanged();
    }

    #region Commands

    public IAsyncRelayCommand SaveCommand { get; }

    #endregion


    #region Bindable properties

    private AccountInfo _accountInfo;
    public AccountInfo AccountDetails
    {
        get => _accountInfo;
        set => SetProperty(ref _accountInfo, value);
    }

    #endregion
    

    protected override async Task OnInitializedAsync()
    {
        await FetchUserAsync();
    }

    private async Task FetchUserAsync()
    {
        var userId = _authService.User?.Id;

        if (userId is null)
            return;
        
        IsLoading = true;
        var result = await _apiClient.GetUserAsync(userId.Value);

        if (result.Success)
        {
            var user = result.Data!;
            AccountDetails = new AccountInfo()
            {
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName,
                PhoneNumber = user.PhoneNumber
            };
        }
        
        IsLoading = false;
    }

    private async Task UpdateAccountAsync()
    {
        IsLoading = true;
        var userId = _authService.User?.Id;
        var result = await _apiClient.UpdateUserAsync(new UpdateUser()
        {
            UserId = userId,
            FirstName = AccountDetails.FirstName,
            LastName = AccountDetails.LastName,
            PhoneNumber = AccountDetails.PhoneNumber,
        });

        if (result.Success)
        {
            await PopupHelpers.ShowSuccessAsync("Account details has been updated successfully");
        }

        IsLoading = false;
    }
}
