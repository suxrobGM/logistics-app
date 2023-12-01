using Microsoft.AspNetCore.Components;

namespace Logistics.AdminApp.Pages;

public partial class Home
{
    #region Injectable services

    [Inject]
    private NavigationManager Navigation { get; set; } = default!;

    #endregion
}
