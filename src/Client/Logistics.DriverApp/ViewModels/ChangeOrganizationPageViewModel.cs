using CommunityToolkit.Mvvm.Messaging;
using Logistics.DriverApp.Messages;
using Logistics.DriverApp.Services;
using Logistics.DriverApp.Services.Authentication;

namespace Logistics.DriverApp.ViewModels;

public class ChangeOrganizationPageVideModel : ViewModelBase
{
    private readonly IApiClient _apiClient;
    private readonly IAuthService _authService;
    private readonly ITenantService _tenantService;

    public ChangeOrganizationPageVideModel(
        IApiClient apiClient, 
        IAuthService authService,
        ITenantService tenantService)
    {
        _authService = authService;
        _apiClient = apiClient;
        _tenantService = tenantService;
        _organizations = Array.Empty<string>();
        // PerformSearchCommand = new AsyncRelayCommand<string>(SearchOrganizationAsync);
        
    }

    // public IAsyncRelayCommand<string> PerformSearchCommand { get; }


    #region Bindable properties

    private string? _currentOrganization;
    public string? CurrentOrganization
    {
        get => _currentOrganization;
        set => SetProperty(ref _currentOrganization, value);
    }

    private string? _selectedOrganization;
    public string? SelectedOrganization
    {
        get => _selectedOrganization;
        set
        {
            SetProperty(ref _selectedOrganization, value);
            
            if (!string.IsNullOrEmpty(value))
            {
                WeakReferenceMessenger.Default.Send(new TenantIdChangedMessage(value));
                Task.Run(async () => await SaveOrganizationIdAsync(value));
            }
        }
    }

    // private string? _searchInput;
    // public string? SearchInput
    // {
    //     get => _searchInput;
    //     set
    //     {
    //         SetProperty(ref _searchInput, value);
    //         Task.Run(async () => await SearchOrganizationAsync(value));
    //     }
    // }

    private string[] _organizations;
    public string[] Organizations
    {
        get => _organizations;
        set => SetProperty(ref _organizations, value);
    }

    #endregion

    protected override async void OnActivated()
    {
        
        await FetchUserOrganizationsAsync();
        
    }

    private async Task FetchUserOrganizationsAsync()
    {
        if (string.IsNullOrEmpty(_authService.User?.Id))
            return;

        IsLoading = true;
        var result = await _apiClient.GetUserOrganizations(_authService.User.Id);

        if (!result.Success)
        {
            await PopupHelpers.ShowErrorAsync(result.Error);
            return;
        }
        
        Organizations = result.Value!.Organizations;

        if (!Organizations.Any())
        {
            await PopupHelpers.ShowErrorAsync(
                "You are not registered with any organization, ask your company to add you to the list of employees as a driver");
            return;
        }
        
        // Automatically redirect to Dashboard page
        if (Organizations.Length == 1)
        {
            CurrentOrganization = Organizations.First();
            await _tenantService.SaveTenantIdAsync(CurrentOrganization);
            await Shell.Current.GoToAsync("//DashboardPage");
        }
        
        IsLoading = false;
    }

    private async Task SaveOrganizationIdAsync(string tenantId)
    {
        await _tenantService.SaveTenantIdAsync(tenantId);
        await MainThread.InvokeOnMainThreadAsync(() => PopupHelpers.ShowSuccessAsync($"Successfully switched to organization: ${tenantId}"));
    }

    // private async Task SearchOrganizationAsync(string? searchInput)
    // {
    //     if (string.IsNullOrEmpty(searchInput))
    //         return;
    //     
    //     var result = await _apiClient.GetTenantsAsync(new SearchableQuery(searchInput));
    //     var hasItems = result.Items?.Any() ?? false;
    //
    //     if (result.Success && hasItems)
    //     {
    //         Organizations = result.Items!.Select(i => i.DisplayName!);
    //     }
    // }
}
