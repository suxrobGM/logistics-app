using Logistics.WebApi.Client.Exceptions;
using Microsoft.AspNetCore.Components;

namespace Logistics.OfficeApp.ViewModels.Pages.Truck;

public class EditTruckViewModel : PageViewModelBase
{

    public EditTruckViewModel(IApiClient apiClient)
        : base(apiClient)
    {
        Truck = new TruckDto();
        _drivers = new List<UserDto>();
    }

    [Parameter]
    public string? Id { get; set; }

    [CascadingParameter]
    public Toast? Toast { get; set; }


    #region Binding properties

    public TruckDto Truck { get; set; }

    private IEnumerable<UserDto> _drivers;
    public IEnumerable<UserDto> Drivers 
    {
        get => _drivers;
        set => SetProperty(ref _drivers, value);
    }

    public bool EditMode => !string.IsNullOrEmpty(Id);

    public string? Error { get; set; }

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

    public override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (!firstRender)
            return;

        var pagedResult = await FetchDriversAsync();
        if (pagedResult.Items != null)
        {
            Drivers = pagedResult.Items;
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
                await apiClient.UpdateTruckAsync(Truck!);
                Toast?.Show("Truck has been saved successfully.", "Notification");
            }
            else
            {
                await apiClient.CreateTruckAsync(Truck!);
                Toast?.Show("A new truck has been created successfully.", "Notification");
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
        Truck.TruckNumber = null;
        Truck.DriverId = null;
    }

    public async Task<IEnumerable<DataListItem>> SearchUser(string value)
    {
        var pagedList = await apiClient.GetUsersAsync(value);
        var dataListItems = new List<DataListItem>();

        if (pagedList.Items != null)
        {
            foreach (var item in pagedList.Items)
            {
                dataListItems.Add(new DataListItem(item.Id!, item.GetFullName()));
            }
        }

        return dataListItems;
    }

    private Task<TruckDto?> FetchTruckAsync(string id)
    {
        return Task.Run(async () =>
        {
            return await apiClient.GetTruckAsync(id);
        });
    }

    private Task<PagedDataResult<UserDto>> FetchDriversAsync()
    {
        return Task.Run(async () =>
        {
            return await apiClient.GetUsersAsync();
        });
    }
}
