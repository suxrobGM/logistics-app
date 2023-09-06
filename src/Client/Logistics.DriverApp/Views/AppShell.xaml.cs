using Plugin.Firebase.CloudMessaging;

namespace Logistics.DriverApp.Views;

public partial class AppShell
{
    public AppShell()
    {
        InitializeComponent();
    }

    protected override async void OnAppearing()
    {
        await CrossFirebaseCloudMessaging.Current.CheckIfValidAsync();
        var token = await CrossFirebaseCloudMessaging.Current.GetTokenAsync();
        Console.WriteLine(token);
    }
}