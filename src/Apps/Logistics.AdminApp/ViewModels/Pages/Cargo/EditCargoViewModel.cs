using Microsoft.AspNetCore.Components;

namespace Logistics.AdminApp.ViewModels.Pages.Cargo;

public class EditCargoViewModel : PageViewModelBase
{

    public EditCargoViewModel(IApiClient apiClient)
        : base(apiClient)
    {
        
    }

    [Parameter]
    public string? Id { get; set; }

    public CargoDto? Cargo { get; set; }

    public override Task OnInitializedAsync()
    {
        return base.OnInitializedAsync();
    }

    //private Task<CargoDto> FetchCargo()
    //{
    //    return Task.Run(async () =>
    //    {
    //        await apiClient.
    //    })
    //}
}
