using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;

using Logistics.DriverApp.Platforms.Android.Consts;

using Plugin.Fingerprint;
using Plugin.Firebase.CloudMessaging;

namespace Logistics.DriverApp.Platforms.Android;

[Activity(Theme = "@style/Maui.SplashTheme",
    MainLauncher = true,
    ConfigurationChanges = ConfigChanges.ScreenSize |
        ConfigChanges.Orientation |
        ConfigChanges.UiMode |
        ConfigChanges.ScreenLayout |
        ConfigChanges.SmallestScreenSize |
        ConfigChanges.Density
)]
public class MainActivity : MauiAppCompatActivity
{
    protected override void OnCreate(Bundle? savedInstanceState)
    {
        base.OnCreate(savedInstanceState);
        CreateNotificationChannels();
        FirebaseCloudMessagingImplementation.OnNewIntent(Intent);
        FirebaseCloudMessagingImplementation.ChannelId = NotificationChannels.GeneralChannelId;
        CrossFingerprint.SetCurrentActivityResolver(() => this);
    }

    protected override void OnNewIntent(Intent? intent)
    {
        base.OnNewIntent(intent);
        FirebaseCloudMessagingImplementation.OnNewIntent(intent);
    }

    private void CreateNotificationChannels()
    {
        var notificationManager = (NotificationManager)GetSystemService(NotificationService)!;
        var generalChanel = new NotificationChannel(NotificationChannels.GeneralChannelId, "General", NotificationImportance.Max);
        var foregroundServiceChannel = new NotificationChannel(NotificationChannels.ForegroundServiceChannelId, "Background Services", NotificationImportance.Default);
        notificationManager.CreateNotificationChannel(generalChanel);
        notificationManager.CreateNotificationChannel(foregroundServiceChannel);
    }
}
