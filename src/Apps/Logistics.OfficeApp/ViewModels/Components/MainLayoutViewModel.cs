namespace Logistics.OfficeApp.ViewModels.Components;

public class MainLayoutViewModel : ViewModelBase
{
    private readonly IHttpContextAccessor _context;
    private readonly IApiClient _apiClient;

    public MainLayoutViewModel(
        IApiClient apiClient, 
        IHttpContextAccessor context)
    {
        _apiClient = apiClient;
        _context = context;
        SetTenant();
    }

    private void SetTenant()
    {
        var tenantCookie = _context?.HttpContext?.Request?.Cookies["X-Tenant"];
        _apiClient.SetCurrentTenantId(tenantCookie);
    }
}
