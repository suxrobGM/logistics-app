using CommunityToolkit.Maui;
using Microsoft.Maui.LifecycleEvents;
using Syncfusion.Maui.Core.Hosting;
using Plugin.Firebase.Auth;
using Plugin.Firebase.Bundled.Shared;
using Plugin.Firebase.CloudMessaging;
using Plugin.Firebase.CloudMessaging.EventArgs;
using Plugin.Firebase.Crashlytics;

#if IOS
    using Plugin.Firebase.Bundled.Platforms.iOS;
#elif ANDROID
    using Plugin.Firebase.Bundled.Platforms.Android;
#endif

namespace Logistics.DriverApp;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .UseMauiCommunityToolkit()
            .ConfigureSyncfusionCore()
            .RegisterFirebaseServices()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
            });

        return builder.Build();
    }
    
    private static MauiAppBuilder RegisterFirebaseServices(this MauiAppBuilder builder)
    {
        var firebaseSettings = new CrossFirebaseSettings(
            isAuthEnabled: true, 
            isCloudMessagingEnabled: true,
            isCrashlyticsEnabled: true);
        
        builder.ConfigureLifecycleEvents(events => {
#if IOS
            events.AddiOS(iOS => iOS.FinishedLaunching((_,__) => {
                CrossFirebase.Initialize(firebaseSettings);
                return false;
            }));
#elif ANDROID
            events.AddAndroid(android => android.OnCreate((activity, _) =>
                CrossFirebase.Initialize(activity, firebaseSettings)));
#endif
        });
        
        CrossFirebaseCloudMessaging.Current.NotificationReceived += CurrentOnNotificationReceived;
        CrossFirebaseCrashlytics.Current.SetCrashlyticsCollectionEnabled(true);
        builder.Services.AddSingleton(_ => CrossFirebaseAuth.Current);
        return builder;
    }

    private static void CurrentOnNotificationReceived(object? sender, FCMNotificationReceivedEventArgs e)
    {
        Console.WriteLine(e.Notification.ToString());
    }
}
