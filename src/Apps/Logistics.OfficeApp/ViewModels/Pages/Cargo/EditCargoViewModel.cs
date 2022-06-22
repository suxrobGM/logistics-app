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

        try
        {
            if (EditMode)
            {
                IsBusy = true;
                var cargo = await apiClient.GetCargoAsync(Id!);

                Cargo = cargo;
                IsBusy = false;
            }
        }
        catch (ApiException e)
        {
            Error = e.Message;
        }
        finally
        {
            IsBusy = false;
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
        IsBusy = true;
        Error = string.Empty;
        try
        {
            if (EditMode)
            {
                await apiClient.UpdateCargoAsync(Cargo!);
                Toast?.Show("Cargo has been saved successfully.", "Notification");
            }
            else
            {
                await apiClient.CreateCargoAsync(Cargo!);
                Toast?.Show("A new cargo has been created successfully.", "Notification");
                ResetData();
            }
        }
        catch (ApiException ex)
        {
            Error = ex.Message;
        }
        finally
        {
            IsBusy = false;
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

        if (!string.IsNullOrEmpty(externalId))
        {
            var user = await apiClient.GetEmployeeAsync(externalId);
            Cargo.AssignedDispatcherId = user.Id;
            Cargo.AssignedDispatcherName = user.GetFullName();
            StateHasChanged();
        }
    }

    public async Task<IEnumerable<DataListItem>> SearchTruck(string value)
    {
        var pagedList = await apiClient.GetTrucksAsync(value);
        var dataListItems = new List<DataListItem>();

        if (pagedList.Items != null)
        {
            foreach (var item in pagedList.Items)
            {
                dataListItems.Add(new DataListItem(item.Id!, item.DriverName!));
            }
        }

        return dataListItems;
    }
}
