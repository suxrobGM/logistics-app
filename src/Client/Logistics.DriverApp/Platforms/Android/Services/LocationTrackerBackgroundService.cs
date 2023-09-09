using Android.App;
using Android.Content;
using Android.OS;
using AndroidX.Core.App;
using Logistics.DriverApp.Platforms.Android.Consts;
using Logistics.DriverApp.Services;
using AndroidApp = Android.App.Application;

namespace Logistics.DriverApp.Platforms.Android.Services;

[Service]
public class LocationTrackerBackgroundService : Service, ILocationTrackerBackgroundService
{
    private const int NotificationId = 1000;
    private readonly ILocationTracker _locationTracker = App.Current.GetRequiredService<ILocationTracker>();
    private Timer? _timer;

    public void Start()
    {
        var intent = new Intent(AndroidApp.Context, typeof(LocationTrackerBackgroundService));
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
        StartForegroundService();
        return StartCommandResult.Sticky;
    }
    
    public override void OnTaskRemoved(Intent? rootIntent)
    {
        // Stop the service when the app is removed from the recent apps list
        StopForeground(StopForegroundFlags.Remove);
        StopSelf();
        OnDestroy();
        System.Diagnostics.Process.GetCurrentProcess().Kill();
    }

    public override void OnDestroy()
    {
        _timer?.Dispose();
        base.OnDestroy();
    }

    private async void SendGeolocationData(object? state)
    {
        await _locationTracker.SendLocationDataAsync();
    }
    
    private void StartForegroundService()
    {
        var notification = new NotificationCompat.Builder(this, NotificationChannels.ForegroundServiceChannelId)
            .SetAutoCancel(false)
            .SetOngoing(true)
            .SetSmallIcon(Resource.Mipmap.appicon)
            .SetContentTitle("JFleets Driver")
            .SetContentText("Driver's geolocation is tracking in real-time")
            .Build();
        
        StartForeground(NotificationId, notification);
        _timer = new Timer(SendGeolocationData, null, TimeSpan.Zero, TimeSpan.FromSeconds(5));
    }
}
