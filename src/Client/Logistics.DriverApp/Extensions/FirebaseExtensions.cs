using Microsoft.Maui.LifecycleEvents;
using Plugin.Firebase.Bundled.Shared;
using Plugin.Firebase.Crashlytics;

#if IOS
using Plugin.Firebase.Bundled.Platforms.iOS;
#elif ANDROID
using Plugin.Firebase.Bundled.Platforms.Android;
#endif

namespace Logistics.DriverApp;

public static class FirebaseExtensions
{
    public static MauiAppBuilder RegisterFirebaseServices(this MauiAppBuilder builder)
    {
        var firebaseSettings = new CrossFirebaseSettings(
            isCloudMessagingEnabled: true,
            isCrashlyticsEnabled: true);
        
        builder.ConfigureLifecycleEvents(events => {
#if IOS
            events.AddiOS(iOS => iOS.FinishedLaunching((_,_) => {
                CrossFirebase.Initialize(firebaseSettings);
                return false;
            }));
#elif ANDROID
            events.AddAndroid(android => android.OnCreate((activity, _) =>
                CrossFirebase.Initialize(activity, firebaseSettings)));
#endif
        });
        
        CrossFirebaseCrashlytics.Current.SetCrashlyticsCollectionEnabled(true);
        return builder;
    }
}
