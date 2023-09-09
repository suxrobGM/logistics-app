namespace Logistics.DriverApp;

public partial class App
{
    private readonly IServiceProvider _serviceProvider;
    
    public App(IServiceProvider serviceProvider)
    {
        InitializeComponent();
        
        _serviceProvider = serviceProvider;
        MainPage = new AppShell();
    }

    public new static App Current => (App)Application.Current!;
    
    public T? GetService<T>() => _serviceProvider.GetService<T>();
    public T GetRequiredService<T>() where T : notnull => _serviceProvider.GetRequiredService<T>();
}
