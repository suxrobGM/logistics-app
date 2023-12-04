using Logistics.Client.Models;
using Logistics.Shared.Policies;
using Logistics.Shared.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;

namespace Logistics.AdminApp.Pages.Tenant;

[Authorize(Policy = Permissions.Tenants.Edit)]
public partial class EditTenant : PageBase
{
    private TenantDto _tenant = new();
    
    
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

        IsLoading = true;
        var tenant = await CallApiAsync(api => api.GetTenantAsync(Id!));

        if (tenant is not null)
        {
            _tenant = tenant;
        }

        IsLoading = false;
    }

    private async Task SubmitAsync()
    {
        IsLoading = true;
        
        if (EditMode)
        {
            var success = await CallApiAsync(api => api.UpdateTenantAsync(new UpdateTenant
            {
                Id = _tenant.Id,
                CompanyName = _tenant.CompanyName
            }));

            if (!success)
            {
                return;
            }
            
            ShowNotification("Tenant has been saved successfully");
        }
        else
        {
            var success = await CallApiAsync(api => api.CreateTenantAsync(new CreateTenant
            {
                Name = _tenant.Name,
                CompanyName = _tenant.CompanyName
            }));

            if (success)
            {
                ShowNotification("A new tenant has been created successfully");
                ResetData();
            }
        }

        IsLoading = false;
    }

    private void ResetData()
    {
        _tenant = new TenantDto();
    }
}
