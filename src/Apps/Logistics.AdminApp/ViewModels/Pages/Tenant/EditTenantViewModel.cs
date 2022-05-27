﻿using Logistics.WebApi.Client.Exceptions;

namespace Logistics.AdminApp.ViewModels.Pages.Tenant;

public class EditTenantViewModel : PageViewModelBase
{
    public EditTenantViewModel(IApiClient apiClient) : base(apiClient)
    {
        Tenant = new TenantDto();
    }

    [Parameter]
    public string? Id { get; set; }

    [CascadingParameter]
    public Toast? Toast { get; set; }

    public TenantDto Tenant { get; set; }
    public bool EditMode => !string.IsNullOrEmpty(Id);
    public string Error { get; set; } = string.Empty;

    public override async Task OnInitializedAsync()
    {
        Error = string.Empty;

        if (EditMode)
        {
            IsBusy = true;
            var tenant = await FetchTenantsAsync(Id!);

            if (tenant != null)
                Tenant = tenant;

            IsBusy = false;
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
        IsBusy = true;
        Error = string.Empty;
        try
        {
            if (EditMode)
            {
                await apiClient.UpdateTenantAsync(Tenant!);
                Toast?.Show("Tenant has been saved successfully.", "Notification");
            }
            else
            {
                await apiClient.CreateTenantAsync(Tenant!);
                Toast?.Show("A new tenant has been created successfully.", "Notification");
                ResetData();
            }
            IsBusy = false;
        }
        catch (ApiException ex)
        {
            Error = ex.Message;
            IsBusy = false;
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
        {
            return;
        }

        var tenant = await Task.Run(async () => await apiClient.GetTenantAsync(Id));
        Tenant.Name = tenant?.Name;
        Tenant.DisplayName = tenant?.DisplayName;
        Tenant.ConnectionString = tenant?.ConnectionString;
        StateHasChanged();
    }

    public async Task<IEnumerable<DataListItem>> SearchTenant(string value)
    {
        var pagedList = await apiClient.GetTenantsAsync(value);
        var dataListItems = new List<DataListItem>();

        if (pagedList.Items != null)
        {
            foreach (var item in pagedList.Items)
            {
                dataListItems.Add(new DataListItem(item.Id!, item.Name!));
            }
        }

        return dataListItems;
    }

    private Task<TenantDto?> FetchTenantsAsync(string id)
    {
        return Task.Run(async () =>
        {
            return await apiClient.GetTenantAsync(id);
        });
    }
}