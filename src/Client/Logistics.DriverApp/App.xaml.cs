namespace Logistics.DriverApp;

public partial class App
{
    private readonly IServiceProvider _serviceProvider;
    
    public App(IServiceProvider serviceProvider)
    {
        InitializeComponent();
        _serviceProvider = serviceProvider;
    }

    protected override Window CreateWindow(IActivationState? activationState)
    {
        return new Window(new AppShell());
    }

    public new static App Current => (App)Application.Current!;
    
    public T? GetService<T>() => _serviceProvider.GetService<T>();
    public T GetRequiredService<T>() where T : notnull => _serviceProvider.GetRequiredService<T>();
}
