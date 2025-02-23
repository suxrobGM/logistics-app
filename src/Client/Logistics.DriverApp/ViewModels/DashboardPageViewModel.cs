using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.Messaging;
using Logistics.DriverApp.Consts;
using Logistics.DriverApp.Messages;
using Logistics.DriverApp.Models;
using Logistics.DriverApp.Services;
using Logistics.DriverApp.Services.Authentication;
using Logistics.DriverApp.Services.LocationTracking;
using Logistics.Shared.Models;
using Plugin.Firebase.CloudMessaging;
using Plugin.Firebase.CloudMessaging.EventArgs;

namespace Logistics.DriverApp.ViewModels;

public class DashboardPageViewModel : BaseViewModel
{
    private readonly IApiClient _apiClient;
    private readonly IAuthService _authService;
    private readonly ILocationTrackerBackgroundService _locationTrackerBackgroundService;
    private readonly ICache _cache;

    public DashboardPageViewModel(
        IApiClient apiClient, 
        IAuthService authService,
        ILocationTrackerBackgroundService locationTrackerBackgroundService,
        ICache cache)
    {
        _apiClient = apiClient;
        _authService = authService;
        _locationTrackerBackgroundService = locationTrackerBackgroundService;
        _cache = cache;
        OpenLoadPageCommand = new AsyncRelayCommand<string?>(OpenLoadPageAsync);
        CrossFirebaseCloudMessaging.Current.NotificationReceived += HandleLoadNotificationReceived;
        
        Messenger.Register<TenantIdChangedMessage>(this, async (_, _) =>
        {
            await MainThread.InvokeOnMainThreadAsync(FetchTruckDataAsync);
        });
        
        Messenger.Register<ActiveLoadsRequestMessage>(this, (_, m) => m.Reply(Loads));
    }

    
    #region Commands

    public IAsyncRelayCommand<string?> OpenLoadPageCommand { get; }

    #endregion


    #region Bindable properties

    public ObservableCollection<ActiveLoad> Loads { get; } = [];

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
        set => SetProperty(ref _teammatesName, value);
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
        var tenantId = _authService.User?.TenantId;
        var truckId = _cache.Get<string>(CacheKeys.TruckId);

        if (string.IsNullOrEmpty(tenantId) || string.IsNullOrEmpty(truckId))
        {
            return;
        }

        _locationTrackerBackgroundService.Start(new LocationTrackerOptions
        {
            TruckId = truckId,
            TenantId = tenantId,
            DriversName = string.Join(", ", DriverName, TeammatesName),
            TruckNumber = TruckNumber
        });
    }

    private async Task OpenLoadPageAsync(string? loadId)
    {
        if (string.IsNullOrEmpty(loadId))
        {
            return;
        }

        var load = Loads.FirstOrDefault(i => i.Id == loadId);

        if (load is null)
        {
            return;
        }

        var parameters = new Dictionary<string, object> { { "load", load } };
        await Shell.Current.GoToAsync("LoadPage", parameters);
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
        
        var result = await _apiClient.GetTruckAsync(new GetTruckQuery
        {
            TruckOrDriverId = driverId,
            IncludeLoads = true,
            OnlyActiveLoads = true,
        });

        if (!result.Success)
        {
            await PopupHelpers.ShowErrorAsync(result.Error);
            IsLoading = false;
            return;
        }

        var truck = result.Data!;
        var teammates = truck.Drivers.Where(i => i.Id != driverId).Select(i => i.FullName);
        TruckNumber = truck.TruckNumber;
        TeammatesName = string.Join(", ", teammates); 
        _cache.Set(CacheKeys.TruckId, truck.Id);

        AddActiveLoads(truck.Loads);
        IsLoading = false;
    }

    private async Task FetchActiveLoadsAsync()
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
        
        AddActiveLoads(result.Data!);
        IsLoading = false;
    }
    
    private async Task SendDeviceTokenAsync()
    {
        await CrossFirebaseCloudMessaging.Current.CheckIfValidAsync();
        var driverId = _authService.User?.Id!;
        var token = await CrossFirebaseCloudMessaging.Current.GetTokenAsync();

        if (!string.IsNullOrEmpty(token))
        {
            await _apiClient.SetDeviceTokenAsync(new SetDeviceToken {UserId = driverId, DeviceToken = token});
        }
    }

    private void AddActiveLoads(IEnumerable<LoadDto> loads)
    {
        var loadsArr = loads.ToArray();
        
        // Update or add loads
        foreach (var loadDto in loadsArr)
        {
            var existingLoad = Loads.FirstOrDefault(i => i.Id == loadDto.Id);
            if (existingLoad != null)
            {
                existingLoad.UpdateFromDto(loadDto);
            }
            else
            {
                Loads.Add(new ActiveLoad(loadDto));
            }
        }

        // Find and remove loads that no longer exist
        var loadsToRemove = Loads.Where(l => loadsArr.All(i => i.Id != l.Id));
        foreach (var loadToRemove in loadsToRemove)
        {
            Loads.Remove(loadToRemove);
        }
        
        OnPropertyChanged(nameof(Loads));
    }
    
    
    #region Event handlers

    private async void HandleLoadNotificationReceived(object? sender, FCMNotificationReceivedEventArgs e)
    {
        if (e.Notification.Data.ContainsKey("loadId"))
        {
            await MainThread.InvokeOnMainThreadAsync(FetchActiveLoadsAsync);
        }
    }

    #endregion
}
