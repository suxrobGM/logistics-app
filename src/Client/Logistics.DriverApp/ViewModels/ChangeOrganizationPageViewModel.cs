using CommunityToolkit.Mvvm.Messaging;
using Logistics.DriverApp.Messages;

namespace Logistics.DriverApp.ViewModels;

public class ChangeOrganizationPageVideModel : ViewModelBase
{
    private readonly IApiClient _apiClient;

    public ChangeOrganizationPageVideModel(IApiClient apiClient)
    {
        _apiClient = apiClient;
        PerformSearchCommand = new AsyncRelayCommand<string>(SearchOrganizationAsync);
    }

    public IAsyncRelayCommand<string> PerformSearchCommand { get; }


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
            }
        }
    }

    private string? _searchInput;
    public string? SearchInput
    {
        get => _searchInput;
        set
        {
            SetProperty(ref _searchInput, value);
            Task.Run(async () => await SearchOrganizationAsync(value));
        }
    }

    private IEnumerable<string>? _organizations;
    public IEnumerable<string>? Organizations
    {
        get => _organizations;
        set => SetProperty(ref _organizations, value);
    }

    #endregion


    private async Task SearchOrganizationAsync(string? searchInput)
    {
        if (string.IsNullOrEmpty(searchInput))
            return;
        
       
        var result = await _apiClient.GetTenantsAsync(new SearchableQuery(searchInput));
        var hasItems = result.Items?.Any() ?? false;

        if (result.Success && hasItems)
        {
            Organizations = result.Items!.Select(i => i.DisplayName!);
        }
    }
}
