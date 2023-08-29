namespace Logistics.DriverApp.Views;

public partial class ChangeOrganizationPage : ContentPage
{
	public ChangeOrganizationPage()
	{
		InitializeComponent();
		BindingContext = App.Current.Services.GetService<ChangeOrganizationPageVideModel>();
	}

	protected override void OnAppearing()
	{
		(BindingContext as ChangeOrganizationPageVideModel)!.IsActive = true;
	}

	protected override void OnDisappearing()
	{
		(BindingContext as ChangeOrganizationPageVideModel)!.IsActive = false;
	}
}