using Logistics.Client.Models;
using Logistics.Shared.Policies;
using Logistics.Shared.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;

namespace Logistics.AdminApp.Pages.Tenant;

[Authorize(Policy = Permissions.Tenants.Edit)]
public partial class EditTenant : PageBase
{
    private TenantDto Tenant { get; set; } = new();
    
    
    #region Parameters

    [Parameter]
    public string? Id { get; set; }

    [Parameter] 
    public bool EditMode { get; set; } = true;

    #endregion
    

    #region Injectable services

    [Inject] 
    private NavigationManager Navigation { get; set; } = default!;

    #endregion
    

    protected override async Task OnInitializedAsync()
    {
        if (!EditMode)
        {
            return;
        }
        
        var tenant = await CallApiAsync(api => api.GetTenantAsync(Id!));

        if (tenant is not null)
        {
            Tenant = tenant;
        }
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (!firstRender)
        {
            return;
        }

        if (!EditMode)
        {
            await LoadCurrentTenantAsync();
        }
    }

    private async Task UpdateAsync()
    {
        if (EditMode)
        {
            var success = await CallApiAsync(api => api.UpdateTenantAsync(new UpdateTenant
            {
                Id = Tenant.Id,
                CompanyName = Tenant.CompanyName
            }));

            if (!success)
            {
                return;
            }
            
            ShowNotification("Tenant has been saved successfully");
        }
        else
        {
            var success = await CallApiAsync(api => api.CreateTenantAsync(new CreateTenant()
            {
                Name = Tenant.Name,
                DisplayName = Tenant.CompanyName
            }));

            if (!success)
            {
                return;
            }
            
            ShowNotification("A new tenant has been created successfully");
            ResetData();
        }

        IsLoading = false;
    }

    private void ResetData()
    {
        Tenant.Name = string.Empty;
        Tenant.CompanyName = string.Empty;
        Tenant.ConnectionString = string.Empty;
    }

    private async Task LoadCurrentTenantAsync()
    {
        if (string.IsNullOrEmpty(Id))
        {
            return;
        }
        
        var tenant = await CallApiAsync(api => api.GetTenantAsync(Id));

        if (tenant is null)
        {
            return;
        }
        
        Tenant.Name = tenant.Name;
        Tenant.CompanyName = tenant.CompanyName;
        Tenant.ConnectionString = tenant.ConnectionString;
    }

    private void BackToTenantsPage()
    {
        Navigation.NavigateTo("/tenants");
    }
}
