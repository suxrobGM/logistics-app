using Logistics.DriverApp.Models;
using Logistics.DriverApp.Services.Authentication;

namespace Logistics.DriverApp.ViewModels;

public class AccountPageViewModel : BaseViewModel
{
    private readonly IAuthService _authService;
    private readonly IApiClient _apiClient;
    private bool _isFetchedData;

    public AccountPageViewModel(
        IAuthService authService, 
        IApiClient apiClient)
    {
        _authService = authService;
        _apiClient = apiClient;
        AccountDetails = new AccountInfo();
        // AccountCenterUrl = $"{_authService.Options.Authority}/account/manage";
        SaveCommand = new AsyncRelayCommand(UpdateAccountAsync, () => !IsLoading);
        IsLoadingChanged += (s, e) => SaveCommand.NotifyCanExecuteChanged();
    }

    public IAsyncRelayCommand SaveCommand { get; }
    public AccountInfo AccountDetails { get; }

    protected override async Task OnInitializedAsync()
    {
        await FetchUserAsync();
    }

    private async Task FetchUserAsync()
    {
        if (_isFetchedData)
            return;
        
        var userId = _authService.User?.Id;

        if (string.IsNullOrEmpty(userId))
            return;
        
        IsLoading = true;
        var result = await _apiClient.GetUserAsync(userId);

        if (result.Success)
        {
            var user = result.Value!;
            AccountDetails.Email = user.Email;
            AccountDetails.FirstName = user.FirstName;
            AccountDetails.LastName = user.LastName;
            AccountDetails.PhoneNumber = user.PhoneNumber;
            _isFetchedData = true;
        }
        
        IsLoading = false;
    }

    private async Task UpdateAccountAsync()
    {
        IsLoading = true;
        var userId = _authService.User?.Id;
        var result = await _apiClient.UpdateUserAsync(new UpdateUser()
        {
            Id = userId,
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
