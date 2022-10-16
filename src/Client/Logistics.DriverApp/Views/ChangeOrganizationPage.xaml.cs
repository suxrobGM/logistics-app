namespace Logistics.DriverApp.Views;

public partial class ChangeOrganizationPage : ContentPage
{
	public ChangeOrganizationPage()
	{
		InitializeComponent();
		BindingContext = App.Current.Services.GetService<ChangeOrganizationPageVideModel>();
	}
}