namespace Logistics.DriverApp.ViewModels;

public class SelectTenantPageVideModel : ViewModelBase
{
    private readonly IApiClient _apiClient;
    private string? _currentCompany;
    private string? _selectedCompany;
    private IEnumerable<string>? _companies;

    public SelectTenantPageVideModel(IApiClient apiClient)
    {
        _apiClient = apiClient;
        PerformSearchCommand = new AsyncRelayCommand<string>(SearchCompanyAsync);
    }

    public IAsyncRelayCommand<string> PerformSearchCommand { get; }

    public string? CurrentCompany
    {
        get => _currentCompany;
        set => SetProperty(ref _currentCompany, value);
    }

    public string? SelectedCompany
    {
        get => _selectedCompany;
        set => SetProperty(ref _selectedCompany, value);
    }

    public IEnumerable<string>? Companies 
    {
        get => _companies;
        set => SetProperty(ref _companies, value);
    }

    private async Task SearchCompanyAsync(string? searchInput)
    {
        var result = await _apiClient.GetTenantsAsync(new SearchableQuery(searchInput));

        if (result.Success)
        {
            Companies = result.Items!.Select(i => i.DisplayName!).ToArray();
        }
    }
}
