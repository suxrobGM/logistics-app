namespace Logistics.AdminApp.Shared;

public partial class MainLayout : LayoutComponentBase
{
    #region Injectable services

    [Inject] 
    private IApiClient ApiClient { get; set; } = default!;

    #endregion
}