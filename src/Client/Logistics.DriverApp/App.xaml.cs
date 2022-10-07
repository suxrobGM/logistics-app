using IdentityModel.OidcClient;
using Microsoft.Extensions.Configuration;

namespace Logistics.DriverApp;

public partial class App : Application
{
    public App()
    {
        InitializeComponent();

        Configuration = BuildConfiguration();
        Services = ConfigureServices();
        MainPage = new AppShell();
    }

    public new static App Current => (App)Application.Current;
    public IServiceProvider Services { get; }
    public IConfiguration Configuration { get; }

    private static IServiceProvider ConfigureServices()
    {
        var services = new ServiceCollection();

        services.AddTransient<MainPageViewModel>();
        services.AddTransient<LoginPageViewModel>();
        services.AddSingleton<OidcClientOptions>();
        return services.BuildServiceProvider();
    }

    private static IConfiguration BuildConfiguration()
    {
        var builder = new ConfigurationBuilder();
        var appsettings = typeof(MauiProgram).Assembly
            .GetManifestResourceStream("Logistics.DriverApp.appsettings.json");

        var secrets = typeof(MauiProgram).Assembly
            .GetManifestResourceStream("Logistics.DriverApp.appsettings.secrets.json");

        builder.AddJsonStream(appsettings);
        builder.AddJsonStream(secrets);
        return builder.Build();
    }

    private void TryAddConfig(this IConfigurationBuilder builder, string configFile)
    {
        try
        {
            var config = typeof(App).Assembly.GetManifestResourceStream($"Logistics.DriverApp.{configFile}");
            builder.AddJsonStream(config);
        }
        catch (Exception)
        {
        }
    }
}