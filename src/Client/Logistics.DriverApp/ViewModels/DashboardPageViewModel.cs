using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.Messaging;
using Logistics.DriverApp.Messages;
using Logistics.DriverApp.Models;
using Logistics.DriverApp.Services;
using Logistics.DriverApp.Services.Authentication;
using Logistics.DriverApp.Services.LocationTracking;
using Logistics.Models;
using Plugin.Firebase.CloudMessaging;
using Plugin.Firebase.CloudMessaging.EventArgs;

namespace Logistics.DriverApp.ViewModels;

public class DashboardPageViewModel : BaseViewModel
{
    private readonly IApiClient _apiClient;
    private readonly IAuthService _authService;
    private readonly IMapsService _mapsService;
    private readonly ILocationTrackerBackgroundService _locationTrackerBackgroundService;
    private string? _truckId;

    public DashboardPageViewModel(
        IApiClient apiClient, 
        IAuthService authService,
        IMapsService mapsService,
        ILocationTrackerBackgroundService locationTrackerBackgroundService)
    {
        _apiClient = apiClient;
        _authService = authService;
        _mapsService = mapsService;
        _locationTrackerBackgroundService = locationTrackerBackgroundService;
        CrossFirebaseCloudMessaging.Current.NotificationReceived += HandleLoadNotificationReceived;
        Messenger.Register<TenantIdChangedMessage>(this, async (_, _) =>
        {
            await MainThread.InvokeOnMainThreadAsync(FetchTruckDataAsync);
        });
    }


    #region Bindable properties
    
    public ObservableCollection<ActiveLoad> ActiveLoads { get; } = new();

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

    private string? _teammatesName;
    public string? TeammatesName
    {
        get => _teammatesName;
        set
        {
            SetProperty(ref _teammatesName, value);
            IsTeammateLabelVisible = !string.IsNullOrEmpty(_teammatesName);
        }
    }

    private bool _isTeammateLabelVisible;
    public bool IsTeammateLabelVisible
    {
        get => _isTeammateLabelVisible;
        set => SetProperty(ref _isTeammateLabelVisible, value);
    }
    
    #endregion

    
    protected override async Task OnInitializedAsync()
    {
        DriverName = _authService.User?.GetFullName();
        await SendDeviceTokenAsync();
        await FetchTruckDataAsync();
        TryStartLocationTrackerService();
    }

    private void TryStartLocationTrackerService()
    {
        var tenantId = _authService.User?.CurrentTenantId;

        if (string.IsNullOrEmpty(tenantId) || string.IsNullOrEmpty(_truckId))
        {
            return;
        }

        _locationTrackerBackgroundService.Start(new LocationTrackerOptions
        {
            TruckId = _truckId,
            TenantId = tenantId,
            DriversName = string.Join(", ", DriverName, TeammatesName),
            TruckNumber = TruckNumber
        });
    }

    private async Task FetchTruckDataAsync()
    {
        IsLoading = true;
        var driverId = _authService.User?.Id;

        if (string.IsNullOrEmpty(driverId))
        {
            await PopupHelpers.ShowErrorAsync("Failed to load driver data, try again");
            IsLoading = false;
            return;
        }
        
        var result = await _apiClient.GetDriverTruckAsync(driverId, false, true);

        if (!result.Success)
        {
            await PopupHelpers.ShowErrorAsync(result.Error);
            IsLoading = false;
            return;
        }

        var truck = result.Value!;
        var teammates = truck.Drivers.Where(i => i.Id != driverId).Select(i => i.FullName);
        TruckNumber = truck.TruckNumber;
        TeammatesName = string.Join(", ", teammates); 
        _truckId = truck.Id;
        
        AddActiveLoads(truck.Loads);
        IsLoading = false;
    }

    private async Task FetchDashboardDataAsync()
    {
        IsLoading = true;
        var driverId = _authService.User?.Id!;
        var result = await _apiClient.GetDriverActiveLoadsAsync(driverId);

        if (!result.Success)
        {
            await PopupHelpers.ShowErrorAsync("Failed to load the dashboard data, try again");
            IsLoading = false;
            return;
        }
        
        var dashboardData = result.Value!;
        AddActiveLoads(dashboardData.ActiveLoads);
        IsLoading = false;
    }
    
    private async Task SendDeviceTokenAsync()
    {
        await CrossFirebaseCloudMessaging.Current.CheckIfValidAsync();
        var driverId = _authService.User?.Id!;
        var token = await CrossFirebaseCloudMessaging.Current.GetTokenAsync();

        if (!string.IsNullOrEmpty(token))
        {
            await _apiClient.SetDeviceTokenAsync(driverId, token);
        }
    }

    private void AddActiveLoads(IEnumerable<LoadDto> loads)
    {
        foreach (var loadDto in loads)
        {
            var existingLoad = ActiveLoads.FirstOrDefault(i => i.LoadData.Id == loadDto.Id);

            if (existingLoad is not null) 
                continue;
            
            var originAddress = $"{loadDto.OriginAddressLat},{loadDto.OriginAddressLong}";
            var destAddress = $"{loadDto.DestinationAddressLat},{loadDto.DestinationAddressLong}";
            var embedMapHtml = _mapsService.GetDirectionsMapHtml(originAddress, destAddress);
            ActiveLoads.Add(new ActiveLoad(loadDto, embedMapHtml));
        }
    }
    
    #region Event handlers

    private async void HandleLoadNotificationReceived(object? sender, FCMNotificationReceivedEventArgs e)
    {
        if (e.Notification.Data.ContainsKey("loadId"))
        {
            await MainThread.InvokeOnMainThreadAsync(FetchDashboardDataAsync);
        }
    }

    #endregion
}
