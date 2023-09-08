using System.Collections.ObjectModel;
using Logistics.DriverApp.Models;
using Logistics.DriverApp.Services;
using Logistics.DriverApp.Services.Authentication;
using Plugin.Firebase.CloudMessaging;
using Plugin.Firebase.CloudMessaging.EventArgs;

namespace Logistics.DriverApp.ViewModels;

public class ActiveLoadsPageViewModel : BaseViewModel
{
    private readonly IApiClient _apiClient;
    private readonly IAuthService _authService;
    private readonly IMapsService _mapsService;

    public ActiveLoadsPageViewModel(
        IApiClient apiClient, 
        IAuthService authService,
        IMapsService mapsService)
    {
        _apiClient = apiClient;
        _authService = authService;
        _mapsService = mapsService;
        CrossFirebaseCloudMessaging.Current.NotificationReceived += HandleLoadNotificationReceived;
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
        await SendDeviceTokenAsync();
        await FetchDashboardDataAsync();
    }

    private async Task FetchDashboardDataAsync()
    {
        IsLoading = true;
        var driverId = _authService.User?.Id!;
        var result = await _apiClient.GetDriverDashboardDataAsync(driverId);

        if (!result.Success)
        {
            await PopupHelpers.ShowErrorAsync("Failed to load the dashboard data, try again");
            IsLoading = false;
            return;
        }
        
        var dashboardData = result.Value!;
        DriverName = dashboardData.DriverFullName;
        TruckNumber = dashboardData.TruckNumber;

        if (dashboardData.ActiveLoads != null)
        {
            foreach (var loadDto in dashboardData.ActiveLoads)
            {
                var embedMapHtml =
                    _mapsService.GetDirectionsMapHtml(loadDto.OriginCoordinates!, loadDto.DestinationCoordinates!);
                ActiveLoads.Add(new ActiveLoad(loadDto, embedMapHtml));
            }
        }
        
        if (dashboardData.TeammatesName != null)
        {
            TeammatesName = string.Join(", ", dashboardData.TeammatesName);
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
            await _apiClient.SetDeviceTokenAsync(driverId, token);
        }
    }

    
    #region Event handlers

    private async void HandleLoadNotificationReceived(object? sender, FCMNotificationReceivedEventArgs e)
    {
        if (e.Notification.Data.ContainsKey("loadId"))
        {
            await FetchDashboardDataAsync();
        }
    }

    #endregion
}
