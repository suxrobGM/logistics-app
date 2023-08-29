namespace Logistics.DriverApp.Views;

public partial class AccountPage : ContentPage
{
	public AccountPage()
	{
		InitializeComponent();
		BindingContext = App.Current.Services.GetService<AccountPageViewModel>();
	}
	
	protected override void OnAppearing()
	{
		(BindingContext as AccountPageViewModel)!.IsActive = true;
	}

	protected override void OnDisappearing()
	{
		(BindingContext as AccountPageViewModel)!.IsActive = false;
	}
}