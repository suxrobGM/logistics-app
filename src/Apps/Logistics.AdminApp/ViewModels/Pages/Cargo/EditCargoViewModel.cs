using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;

namespace Logistics.AdminApp.ViewModels.Pages.Cargo;

public class EditCargoViewModel : PageViewModelBase
{
    private readonly AuthenticationStateProvider authStateProvider;

    public EditCargoViewModel(
        AuthenticationStateProvider authStateProvider,
        IApiClient apiClient)
        : base(apiClient)
    {
        this.authStateProvider = authStateProvider;
        Trucks = new List<TruckDto>();
        Cargo = new CargoDto();
    }

    [Parameter]
    public string? Id { get; set; }

    [CascadingParameter]
    public Toast? Toast { get; set; }


    #region Binding properties

    public CargoDto Cargo { get; set; }
    public IEnumerable<TruckDto> Trucks { get; set; }
    public bool EditMode => !string.IsNullOrEmpty(Id);

    #endregion


    public override async Task OnInitializedAsync()
    {
        if (EditMode)
        {
            IsBusy = true;
            var cargo = await FetchCargoAsync(Id!);

            if (cargo != null)
                Cargo = cargo;

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
        if (EditMode)
        {
            await Task.Run(async () => await apiClient.UpdateCargoAsync(Cargo!));
            Toast?.Show("Cargo has been saved successfully.", "Notification");
        }
        else
        {
            await Task.Run(async () => await apiClient.CreateCargoAsync(Cargo!));
            Toast?.Show("A new cargo has been created successfully.", "Notification");
        }
        IsBusy = false;
    }

    private async Task LoadCurrentDispatcherAsync()
    {
        var authState = await authStateProvider.GetAuthenticationStateAsync();
        var externalId = authState.User.GetId();

        if (!string.IsNullOrEmpty(externalId) && Cargo != null)
        {
            var user = await Task.Run(async () => await apiClient.GetUserAsync(externalId));
            Cargo.AssignedDispatcherId = user.Id;
            Cargo.AssignedDispatcherName = user.GetFullName();
            StateHasChanged();
        }
    }

    public IEnumerable<string> SearchDriverName(string value)
    {
        return new List<string>();
    }

    private Task<CargoDto?> FetchCargoAsync(string id)
    {
        return Task.Run(async () =>
        {
            return await apiClient.GetCargoAsync(id);
        });
    }

    private Task<PagedDataResult<TruckDto>> FetchTrucksAsync()
    {
        return Task.Run(async () =>
        {
            return await apiClient.GetTrucksAsync();
        });
    }
}
