using Microsoft.AspNetCore.Components.Authorization;

namespace Logistics.OfficeApp.ViewModels.Pages.Load;

public class EditLoadViewModel : PageViewModelBase
{
    private readonly AuthenticationStateProvider _authStateProvider;

    public EditLoadViewModel(
        AuthenticationStateProvider authStateProvider,
        IApiClient apiClient)
        : base(apiClient)
    {
        _authStateProvider = authStateProvider;
        Load = new LoadDto();
    }

    [Parameter]
    public string? Id { get; set; }


    #region Binding properties

    public LoadDto Load { get; set; }
    public bool EditMode => !string.IsNullOrEmpty(Id);
    

    #endregion
    
    public override async Task OnInitializedAsync()
    {
        await base.OnInitializedAsync();

        if (EditMode)
        {
            var result = await CallApi(i => i.GetLoadAsync(Id!));

            if (!result.Success)
                return;
                
            Load = result.Value!;
        }
    }

    public override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (!firstRender)
            return;

        if (!EditMode)
        {
            await LoadCurrentDispatcherAsync();
        }
    }

    public async Task UpdateAsync()
    {
        Error = string.Empty;
        
        if (EditMode)
        {
            var result = await CallApi(i => i.UpdateLoadAsync(Load));

            if (!result.Success)
                return;
                
            Toast?.Show("Cargo has been saved successfully.", "Notification");
        }
        else
        {
            var result = await CallApi(i => i.CreateLoadAsync(Load));

            if (!result.Success)
                return;
                
            Toast?.Show("A new cargo has been created successfully.", "Notification");
            ResetData();
        }
    }

    private void ResetData()
    {
        Load.AssignedTruckId = null;
        Load.PricePerMile = 0;
        Load.TotalTripMiles = 0;
        Load.Source = string.Empty;
        Load.Destination = string.Empty;
    }

    private async Task LoadCurrentDispatcherAsync()
    {
        var authState = await _authStateProvider.GetAuthenticationStateAsync();
        var externalId = authState.User.GetId();

        if (string.IsNullOrEmpty(externalId))
            return;
        
        var result = await CallApi(i => i.GetEmployeeAsync(externalId));

        if (!result.Success)
            return;

        var employee = result.Value!;
        Load.AssignedDispatcherId = employee.Id;
        Load.AssignedDispatcherName = employee.GetFullName();
        StateHasChanged();
    }

    public async Task<IEnumerable<DropdownItem>> SearchTruck(string searchInput)
    {
        var result = await CallApi(i => i.GetTrucksAsync(searchInput));
        var pagedList = result.Value;
        var dataListItems = new List<DropdownItem>();

        if (pagedList?.Items != null)
        {
            foreach (var item in pagedList.Items)
            {
                dataListItems.Add(new DropdownItem(item.Id!, item.DriverName!));
            }
        }

        return dataListItems;
    }
}
