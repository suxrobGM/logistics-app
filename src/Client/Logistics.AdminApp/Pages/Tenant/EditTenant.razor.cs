using Microsoft.AspNetCore.Authorization;

namespace Logistics.AdminApp.Pages.Tenant;

[Authorize(Policy = Permissions.Tenant.Edit)]
public partial class EditTenant : PageBase
{
    #region Parameters

    [Parameter]
    public string? Id { get; set; }

    #endregion


    private Client.Models.Tenant Tenant { get; set; } = new();
    private bool EditMode => !string.IsNullOrEmpty(Id);

    protected override async Task OnInitializedAsync()
    {
        if (!EditMode)
            return;

        IsBusy = true;
        var result = await ApiClient.GetTenantAsync(Id!);

        if (!result.Success)
            return;

        Tenant = result.Value!;
        IsBusy = false;
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (!firstRender)
            return;

        if (!EditMode)
        {
            await LoadCurrentTenantAsync();
        }
    }

    private async Task UpdateAsync()
    {
        Error = string.Empty;
        IsBusy = true;
        
        if (EditMode)
        {
            var result = await ApiClient.UpdateTenantAsync(new UpdateTenant()
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
            var result = await ApiClient.CreateTenantAsync(new CreateTenant()
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
        var result = await ApiClient.GetTenantAsync(Id);

        if (!result.Success)
            return;
        
        var tenant = result.Value!;
        Tenant.Name = tenant.Name;
        Tenant.DisplayName = tenant.DisplayName;
        Tenant.ConnectionString = tenant.ConnectionString;
        IsBusy = false;
    }
}