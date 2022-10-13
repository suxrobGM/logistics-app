namespace Logistics.DriverApp.Views;

public partial class DashboardPage : ContentPage
{
    public DashboardPage()
    {
        InitializeComponent();
        BindingContext = App.Current.Services.GetService<DashboardPageViewModel>();
    }
}