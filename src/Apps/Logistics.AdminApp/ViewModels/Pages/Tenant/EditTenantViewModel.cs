namespace Logistics.AdminApp.ViewModels.Pages.Tenant;

public class EditTenantViewModel : PageViewModelBase
{
    public EditTenantViewModel(IApiClient apiClient) : base(apiClient)
    {
        Tenant = new TenantDto();
    }

    [Parameter]
    public string? Id { get; set; }

    public TenantDto Tenant { get; set; }
    public bool EditMode => !string.IsNullOrEmpty(Id);

    public override async Task OnInitializedAsync()
    {
        await base.OnInitializedAsync();

        if (EditMode)
        {
            var result = await CallApi(i => i.GetTenantAsync(Id!));

            if (!result.Success)
                return;

            Tenant = result.Value!;
        }
    }

    public override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (!firstRender)
            return;

        if (!EditMode)
        {
            await LoadCurrentTenantAsync();
        }
    }

    public async Task UpdateAsync()
    {
        Error = string.Empty;
        
        if (EditMode)
        {
            var result = await CallApi(i => i.UpdateTenantAsync(Tenant));

            if (!result.Success)
                return;
            
            Toast?.Show("Tenant has been saved successfully.", "Notification");
        }
        else
        {
            var result = await CallApi(i => i.CreateTenantAsync(Tenant));
            
            if (!result.Success)
                return;
            
            Toast?.Show("A new tenant has been created successfully.", "Notification");
            ResetData();
        }
    }

    private void ResetData()
    {
        Tenant.Name = string.Empty;
        Tenant.DisplayName = string.Empty;
        Tenant.ConnectionString = string.Empty;
    }

    private async Task LoadCurrentTenantAsync()
    {
        if (string.IsNullOrEmpty(Id))
            return;

        var result = await CallApi(i => i.GetTenantAsync(Id));

        if (!result.Success)
            return;
        
        var tenant = result.Value!;
        Tenant.Name = tenant.Name;
        Tenant.DisplayName = tenant.DisplayName;
        Tenant.ConnectionString = tenant.ConnectionString;
        StateHasChanged();
    }
}
