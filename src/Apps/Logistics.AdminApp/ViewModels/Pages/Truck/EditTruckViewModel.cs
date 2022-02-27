using Logistics.AdminApp.Shared.Components;
using Microsoft.AspNetCore.Components;

namespace Logistics.AdminApp.ViewModels.Pages.Truck;

public class EditTruckViewModel : PageViewModelBase
{

    public EditTruckViewModel(IApiClient apiClient)
        : base(apiClient)
    {
        Truck = new TruckDto();
    }

    [Parameter]
    public string? Id { get; set; }

    [CascadingParameter]
    public Toast? Toast { get; set; }


    #region Binding properties

    public TruckDto Truck { get; set; }
    public bool EditMode => !string.IsNullOrEmpty(Id);

    #endregion


    public override async Task OnInitializedAsync()
    {
        if (EditMode)
        {
            IsBusy = true;
            var truck = await FetchTruckAsync(Id!);

            if (truck != null)
                Truck = truck;

            IsBusy = false;
        }
    }

    public async Task UpdateAsync()
    {
        IsBusy = true;
        if (EditMode)
        {
            await Task.Run(async () => await apiClient.UpdateTruckAsync(Truck!));
            Toast?.Show("Truck has been saved successfully.", "Notification");
        }
        else
        {
            await Task.Run(async () => await apiClient.CreateTruckAsync(Truck!));
            Toast?.Show("A new truck has been created successfully.", "Notification");
        }
        IsBusy = false;
    }

    private Task<TruckDto?> FetchTruckAsync(string id)
    {
        return Task.Run(async () =>
        {
            return await apiClient.GetTruckAsync(id);
        });
    }
}
