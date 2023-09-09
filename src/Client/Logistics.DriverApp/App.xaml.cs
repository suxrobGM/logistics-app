namespace Logistics.DriverApp;

public partial class App
{
    public App(IServiceProvider serviceProvider)
    {
        InitializeComponent();
        
        Services = serviceProvider;
        MainPage = new AppShell();
    }

    public new static App Current => (App)Application.Current!;
    public IServiceProvider Services { get; set; }
}
