namespace Logistics.AdminApp.Pages;

public partial class Logout : PageBase
{
    #region Injectable services

    [Inject] 
    private NavigationManager NavigationManager { get; set; } = default!;

    #endregion
    
    private void SignOut()
    {
        NavigationManager.NavigateTo("/Account/Logout", true);
    }
}