namespace Logistics.OfficeApp.ViewModels.Components;

public class NavMenuViewModel : ViewModelBase
{
    private readonly IApiClient _apiClient;

    public NavMenuViewModel(IApiClient apiClient)
    {
        _apiClient = apiClient;
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

        TenantDisplayName = await _apiClient.GetTenantDisplayNameAsync(_apiClient.CurrentTenantId!);
        await base.OnAfterRenderAsync(firstRender);
    }
}
