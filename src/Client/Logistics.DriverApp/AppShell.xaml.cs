namespace Logistics.DriverApp;

public partial class AppShell : Shell
{
    public AppShell()
    {
        InitializeComponent();
        BindingContext = App.Current.Services.GetService<AppShellViewModel>();
    }
}