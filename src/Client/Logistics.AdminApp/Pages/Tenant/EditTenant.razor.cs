using Logistics.Models;
using Microsoft.AspNetCore.Authorization;

namespace Logistics.AdminApp.Pages.Tenant;

[Authorize(Policy = Permissions.Tenants.Edit)]
public partial class EditTenant : PageBase
{
    #region Parameters

    [Parameter]
    public string? Id { get; set; }

    #endregion


    private TenantDto Tenant { get; set; } = new();
    private bool EditMode => !string.IsNullOrEmpty(Id);

    protected override async Task OnInitializedAsync()
    {
        if (!EditMode)
            return;
        
        var tenant = await CallApiAsync(api => api.GetTenantAsync(Id!));

        if (tenant != null)
        {
            Tenant = tenant;
        }
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
        if (EditMode)
        {
            var success = await CallApiAsync(api => api.UpdateTenantAsync(new UpdateTenant()
            {
                Id = Tenant.Id,
                DisplayName = Tenant.DisplayName
            }));
            
            if (!success)
                return;
            
            Toast?.Show("Tenant has been saved successfully.", "Notification");
        }
        else
        {
            var success = await CallApiAsync(api => api.CreateTenantAsync(new CreateTenant()
            {
                Name = Tenant.Name,
                DisplayName = Tenant.DisplayName
            }));
            
            if (!success)
                return;
            
            Toast?.Show("A new tenant has been created successfully.", "Notification");
            ResetData();
        }

        IsLoading = false;
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
        
        var tenant = await CallApiAsync(api => api.GetTenantAsync(Id));

        if (tenant == null)
            return;
        
        Tenant.Name = tenant.Name;
        Tenant.DisplayName = tenant.DisplayName;
        Tenant.ConnectionString = tenant.ConnectionString;
    }
}