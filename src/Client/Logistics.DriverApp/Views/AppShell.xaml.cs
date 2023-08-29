namespace Logistics.DriverApp.Views;

public partial class AppShell : Shell
{
    public AppShell()
    {
        InitializeComponent();
        BindingContext = App.Current.Services.GetService<AppShellViewModel>();
    }
}