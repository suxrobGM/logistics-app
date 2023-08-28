using Logistics.DriverApp.Services.Authentication;

namespace Logistics.DriverApp.ViewModels;

public class DashboardPageViewModel : ViewModelBase
{
    private readonly IApiClient _apiClient;
    private readonly IAuthService _authService;

    public DashboardPageViewModel(
        IApiClient apiClient, 
        IAuthService authService)
    {
        _apiClient = apiClient;
        _authService = authService;
        DriverName = _authService.User?.UserName;
        Task.Run(FetchTruckAsync);
    }

    public int? _truckNumber;
    public int? TruckNumber
    {
        get => _truckNumber;
        set => SetProperty(ref _truckNumber, value);
    }

    public string? _driverName;
    public string? DriverName
    {
        get => _driverName;
        set => SetProperty(ref _driverName, value);
    }

    private async Task FetchTruckAsync()
    {
        var driverId = _authService.User?.Id;
        var result = await _apiClient.GetTruckByDriverAsync(driverId!);

        if (result.Success)
        {
            TruckNumber = result.Value!.TruckNumber;
        }
    }
}
