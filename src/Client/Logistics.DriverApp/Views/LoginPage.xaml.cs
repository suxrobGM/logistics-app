namespace Logistics.DriverApp.Views;

public partial class LoginPage : ContentPage
{
	public LoginPage()
	{
		InitializeComponent();
		BindingContext = App.Current.Services.GetService<LoginPageViewModel>();
	}
}