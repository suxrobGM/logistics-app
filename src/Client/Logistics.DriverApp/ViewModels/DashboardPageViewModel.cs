using Logistics.DriverApp.Services.Authentication;

namespace Logistics.DriverApp.ViewModels;

public class DashboardPageViewModel : BaseViewModel
{
    private readonly IApiClient _apiClient;
    private readonly IAuthService _authService;

    public DashboardPageViewModel(
        IApiClient apiClient, 
        IAuthService authService)
    {
        _apiClient = apiClient;
        _authService = authService;
    }
    
    
    #region Bindable properties

    private string? _truckNumber;
    public string? TruckNumber
    {
        get => _truckNumber;
        set => SetProperty(ref _truckNumber, value);
    }

    private string? _driverName;
    public string? DriverName
    {
        get => _driverName;
        set => SetProperty(ref _driverName, value);
    }

    #endregion

    protected override async Task OnAppearingAsync()
    {
        await FetchDashboardDataAsync();
    }

    private async Task FetchDashboardDataAsync()
    {
        var driverId = _authService.User?.Id;
        var result = await _apiClient.GetDriverDashboardDataAsync(driverId!);

        if (!result.Success)
            return;
        
        DriverName = result.Value!.DriverFullName;
        TruckNumber = result.Value.TruckNumber;
    }
}
