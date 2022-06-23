namespace Logistics.OfficeApp.ViewModels.Pages.Truck;

public class EditTruckViewModel : PageViewModelBase
{
    public EditTruckViewModel(IApiClient apiClient)
        : base(apiClient)
    {
        Truck = new TruckDto();
        _drivers = new List<EmployeeDto>();
    }

    [Parameter]
    public string? Id { get; set; }


    #region Binding properties

    public TruckDto Truck { get; set; }

    private IEnumerable<EmployeeDto> _drivers;
    public IEnumerable<EmployeeDto> Drivers 
    {
        get => _drivers;
        set => SetProperty(ref _drivers, value);
    }

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
                Truck = await apiClient.GetTruckAsync(Id!);
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

        try
        {
            var pagedResult = await apiClient.GetEmployeesAsync();
            if (pagedResult.Items != null)
            {
                Drivers = pagedResult.Items;
            }
        }
        catch (ApiException e)
        {
            Error = e.Message;
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
        Truck.TruckNumber = null;
        Truck.DriverId = null;
    }

    public async Task<IEnumerable<DataListItem>> SearchUser(string value)
    {
        var pagedList = await apiClient.GetEmployeesAsync(value);
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
}
