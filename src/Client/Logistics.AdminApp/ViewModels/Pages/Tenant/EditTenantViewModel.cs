namespace Logistics.AdminApp.ViewModels.Pages;

public class EditTenantViewModel : PageBaseViewModel
{
    private readonly IApiClient _apiClient;

    public EditTenantViewModel(IApiClient apiClient)
    {
        _apiClient = apiClient;
        Tenant = new Tenant();
    }

    [Parameter]
    public string? Id { get; set; }

    public Tenant Tenant { get; set; }
    public bool EditMode => !string.IsNullOrEmpty(Id);

    public override async Task OnInitializedAsync()
    {
        await base.OnInitializedAsync();

        if (!EditMode)
            return;

        IsBusy = true;
        var result = await _apiClient.GetTenantAsync(Id!);

        if (!result.Success)
            return;

        Tenant = result.Value!;
        IsBusy = false;
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
        IsBusy = true;
        
        if (EditMode)
        {
            var result = await _apiClient.UpdateTenantAsync(new UpdateTenant()
            {
                Id = Tenant.Id,
                DisplayName = Tenant.DisplayName
            });

            if (!result.Success)
                return;
            
            Toast?.Show("Tenant has been saved successfully.", "Notification");
        }
        else
        {
            var result = await _apiClient.CreateTenantAsync(new CreateTenant()
            {
                Name = Tenant.Name,
                DisplayName = Tenant.DisplayName
            });
            
            if (!result.Success)
                return;
            
            Toast?.Show("A new tenant has been created successfully.", "Notification");
            ResetData();
        }

        IsBusy = false;
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

        IsBusy = true;
        var result = await _apiClient.GetTenantAsync(Id);

        if (!result.Success)
            return;
        
        var tenant = result.Value!;
        Tenant.Name = tenant.Name;
        Tenant.DisplayName = tenant.DisplayName;
        Tenant.ConnectionString = tenant.ConnectionString;
        IsBusy = false;
    }
}
