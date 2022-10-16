namespace Logistics.DriverApp.ViewModels;

public class ChangeOrganizationPageVideModel : ViewModelBase
{
    private readonly IApiClient _apiClient;
    private string? _currentOrganization;
    private string? _selectedOrganization;
    private string? _searchInput;
    private IEnumerable<string>? _organizations;

    public ChangeOrganizationPageVideModel(IApiClient apiClient)
    {
        _apiClient = apiClient;
        PerformSearchCommand = new AsyncRelayCommand<string>(SearchOrganizationAsync);
    }

    public IAsyncRelayCommand<string> PerformSearchCommand { get; }

    public string? CurrentOrganization
    {
        get => _currentOrganization;
        set => SetProperty(ref _currentOrganization, value);
    }

    public string? SelectedOrganization
    {
        get => _selectedOrganization;
        set => SetProperty(ref _selectedOrganization, value);
    }

    public string? SearchInput
    {
        get => _searchInput;
        set
        {
            SetProperty(ref _searchInput, value);

        }
    }

    public IEnumerable<string>? Organizations 
    {
        get => _organizations;
        set => SetProperty(ref _organizations, value);
    }

    private async Task SearchOrganizationAsync(string? searchInput)
    {
        if (string.IsNullOrEmpty(searchInput))
            return;
        
        var result = await _apiClient.GetTenantsAsync(new SearchableQuery(searchInput));
        var hasItems = result.Items?.Any() ?? false;

        if (result.Success && hasItems)
        {
            Organizations = result.Items!.Select(i => i.DisplayName!).ToArray();
        }
    }
}
