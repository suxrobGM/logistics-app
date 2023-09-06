using Logistics.DriverApp.Services.Authentication;
using Logistics.Models;
using Plugin.Firebase.CloudMessaging;

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

    private LoadDto? _currentLoad;
    public LoadDto? CurrentLoad
    {
        get => _currentLoad;
        set
        {
            SetProperty(ref _currentLoad, value);
            IsCurrentLoadVisible = _currentLoad != null;
        }
    }

    private bool _isCurrentLoadVisible;
    public bool IsCurrentLoadVisible
    {
        get => _isCurrentLoadVisible;
        set
        {
            SetProperty(ref _isCurrentLoadVisible, value);
            IsNoLoadsLabelVisible = !_isCurrentLoadVisible;
        }
    }

    private bool _isNoLoadsLabelVisible;
    public bool IsNoLoadsLabelVisible
    {
        get => _isNoLoadsLabelVisible;
        set => SetProperty(ref _isNoLoadsLabelVisible, value);
    }
    
    #endregion

    
    protected override async Task OnInitializedAsync()
    {
        await SendDeviceTokenAsync();
        await FetchDashboardDataAsync();
    }

    private async Task FetchDashboardDataAsync()
    {
        IsLoading = true;
        var driverId = _authService.User?.Id!;
        var result = await _apiClient.GetDriverDashboardDataAsync(driverId);

        if (result.Success)
        {
            DriverName = result.Value!.DriverFullName;
            TruckNumber = result.Value.TruckNumber;
            CurrentLoad = result.Value.AssignedLoad;
        }
        
        IsLoading = false;
    }
    
    private async Task SendDeviceTokenAsync()
    {
        await CrossFirebaseCloudMessaging.Current.CheckIfValidAsync();
        var driverId = _authService.User?.Id!;
        var token = await CrossFirebaseCloudMessaging.Current.GetTokenAsync();

        if (!string.IsNullOrEmpty(token))
        {
            await _apiClient.SetDriverDeviceTokenAsync(driverId, token);
        }
    }
}
