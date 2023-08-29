namespace Logistics.DriverApp.Views;

public partial class AccountPage : ContentPage
{
	public AccountPage()
	{
		InitializeComponent();
		BindingContext = App.Current.Services.GetService<AccountPageViewModel>();
	}
}