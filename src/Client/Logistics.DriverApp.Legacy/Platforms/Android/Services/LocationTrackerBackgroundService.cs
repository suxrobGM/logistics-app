using System.Text.Json;

using Android.App;
using Android.Content;
using Android.OS;

using AndroidX.Core.App;

using Logistics.DriverApp.Platforms.Android.Consts;
using Logistics.DriverApp.Services;
using Logistics.DriverApp.Services.LocationTracking;

using AndroidApp = Android.App.Application;

namespace Logistics.DriverApp.Platforms.Android.Services;

[Service]
public class LocationTrackerBackgroundService : Service, ILocationTrackerBackgroundService
{
    private const int TimerSeconds = 30;
    private const int NotificationId = 1000;
    private readonly ILocationTracker _locationTracker = App.Current.GetRequiredService<ILocationTracker>();
    private readonly ILoadProximityUpdater _proximityUpdater = App.Current.GetRequiredService<ILoadProximityUpdater>();
    private Timer? _timer;
    private LocationTrackerOptions? _options;

    public void Start(LocationTrackerOptions options)
    {
        var intent = new Intent(AndroidApp.Context, typeof(LocationTrackerBackgroundService));
        var serializedOptions = JsonSerializer.Serialize(options);
        intent.PutExtra(nameof(LocationTrackerOptions), serializedOptions);
        AndroidApp.Context.StartForegroundService(intent);
    }

    public void Stop()
    {
        var intent = new Intent(AndroidApp.Context, typeof(LocationTrackerBackgroundService));
        AndroidApp.Context.StopService(intent);
    }

    public override IBinder? OnBind(Intent? intent)
    {
        return null;
    }

    public override StartCommandResult OnStartCommand(Intent? intent, StartCommandFlags flags, int startId)
    {
        var optionsStr = intent?.GetStringExtra(nameof(LocationTrackerOptions));

        if (!string.IsNullOrEmpty(optionsStr))
        {
            _options = JsonSerializer.Deserialize<LocationTrackerOptions>(optionsStr);
        }

        if (_options != null)
        {
            StartForegroundService();
        }

        return StartCommandResult.Sticky;
    }

    public override void OnTaskRemoved(Intent? rootIntent)
    {
        // Stop the service when the app is removed from the recent apps list
        StopForeground(StopForegroundFlags.Remove);
        StopSelf();
        DisposeResources();
        System.Diagnostics.Process.GetCurrentProcess().Kill();
        base.OnTaskRemoved(rootIntent);
    }

    public override void OnDestroy()
    {
        DisposeResources();
        base.OnDestroy();
    }

    private async void SendGeolocationData(object? state)
    {
        var currentLocation = await _locationTracker.SendLocationDataAsync(_options!);

        if (currentLocation is not null)
        {
            await _proximityUpdater.UpdateLoadProximitiesAsync(currentLocation);
        }
    }

    private void StartForegroundService()
    {
        var notification = new NotificationCompat.Builder(this, NotificationChannels.ForegroundServiceChannelId)
            .SetAutoCancel(false)
            .SetOngoing(true)
            .SetSmallIcon(Resource.Mipmap.appicon)
            .SetContentTitle("Real-time tracking")
            .SetContentText("Driver's geolocation is tracking in real-time")
            .Build();

        StartForeground(NotificationId, notification);
        _timer = new Timer(SendGeolocationData, null, TimeSpan.Zero, TimeSpan.FromSeconds(TimerSeconds));
    }

    private void DisposeResources()
    {
        _timer?.Dispose();
    }
}
