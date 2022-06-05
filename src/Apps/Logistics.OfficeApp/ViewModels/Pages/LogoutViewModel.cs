namespace Logistics.OfficeApp.ViewModels.Pages;

public class LogoutViewModel : PageViewModelBase
{
    private readonly NavigationManager _navigationManager;

    public LogoutViewModel(
        NavigationManager navigationManager, 
        IApiClient apiClient) 
        : base(apiClient)
    {
        _navigationManager = navigationManager;
    } 

    public void SignOut()
    {
        _navigationManager.NavigateTo("/MicrosoftIdentity/Account/SignOut", true);
    }
}
