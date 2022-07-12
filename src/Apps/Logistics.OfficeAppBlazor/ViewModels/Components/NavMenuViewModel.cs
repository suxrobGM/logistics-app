namespace Logistics.OfficeApp.ViewModels.Components;

public class NavMenuViewModel : ViewModelBase
{
    private readonly IApiClient _apiClient;
    private readonly IHttpContextAccessor _httpContext;

    public NavMenuViewModel(IApiClient apiClient, IHttpContextAccessor httpContext)
    {
        _apiClient = apiClient;
        _httpContext = httpContext;
    }

    private string? _tenantDisplayName;
    public string? TenantDisplayName 
    {
        get => _tenantDisplayName;
        set => SetProperty(ref _tenantDisplayName, value);
    }

    public override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (!firstRender)
            return;

        var tenantId = _httpContext.HttpContext?.GetTenantId();
        TenantDisplayName = await _apiClient.GetTenantDisplayNameAsync(tenantId!);
        await base.OnAfterRenderAsync(firstRender);
    }
}
