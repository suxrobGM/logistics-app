namespace Logistics.DriverApp.Views;

public partial class SelectTenantPage : ContentPage
{
	public SelectTenantPage()
	{
		InitializeComponent();
		BindingContext = App.Current.Services.GetService<SelectTenantPageVideModel>();
	}
}