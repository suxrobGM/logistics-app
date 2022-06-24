using Microsoft.AspNetCore.Components.Authorization;

namespace Logistics.OfficeApp.ViewModels.Pages.Cargo;

public class EditCargoViewModel : PageViewModelBase
{
    private readonly AuthenticationStateProvider _authStateProvider;

    public EditCargoViewModel(
        AuthenticationStateProvider authStateProvider,
        IApiClient apiClient)
        : base(apiClient)
    {
        _authStateProvider = authStateProvider;
        Cargo = new CargoDto();
    }

    [Parameter]
    public string? Id { get; set; }


    #region Binding properties

    public CargoDto Cargo { get; set; }
    public bool EditMode => !string.IsNullOrEmpty(Id);
    

    #endregion
    
    public override async Task OnInitializedAsync()
    {
        await base.OnInitializedAsync();

        if (EditMode)
        {
            var result = await CallApi(i => i.GetCargoAsync(Id!));

            if (!result.Success)
                return;
                
            Cargo = result.Value!;
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
            var result = await CallApi(i => i.UpdateCargoAsync(Cargo));

            if (!result.Success)
                return;
                
            Toast?.Show("Cargo has been saved successfully.", "Notification");
        }
        else
        {
            var result = await CallApi(i => i.CreateCargoAsync(Cargo));

            if (!result.Success)
                return;
                
            Toast?.Show("A new cargo has been created successfully.", "Notification");
            ResetData();
        }
    }

    private void ResetData()
    {
        Cargo.AssignedTruckId = null;
        Cargo.PricePerMile = 0;
        Cargo.TotalTripMiles = 0;
        Cargo.Source = string.Empty;
        Cargo.Destination = string.Empty;
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
        Cargo.AssignedDispatcherId = employee.Id;
        Cargo.AssignedDispatcherName = employee.GetFullName();
        StateHasChanged();
    }

    public async Task<IEnumerable<DataListItem>> SearchTruck(string searchInput)
    {
        var result = await CallApi(i => i.GetTrucksAsync(searchInput));
        var pagedList = result.Value;
        var dataListItems = new List<DataListItem>();

        if (pagedList?.Items != null)
        {
            foreach (var item in pagedList.Items)
            {
                dataListItems.Add(new DataListItem(item.Id!, item.DriverName!));
            }
        }

        return dataListItems;
    }
}
