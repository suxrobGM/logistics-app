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
        
        if (EditMode)
        {
            var result = await CallApi(i => i.GetTruckAsync(Id!));

            if (!result.Success)
                return;
            
            Truck = result.Value!;
        }
    }

    public override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (!firstRender)
            return;

        var result = await CallApi(i => i.GetEmployeesAsync());

        if (!result.Success)
            return;

        var pagedResult = result.Value;
        if (pagedResult?.Items != null)
        {
            Drivers = pagedResult.Items;
        }
    }

    public async Task UpdateAsync()
    {
        Error = string.Empty;

        if (EditMode)
        {
            var result = await CallApi(i => i.UpdateTruckAsync(Truck));

            if (!result.Success)
                return;
            
            Toast?.Show("Truck has been saved successfully.", "Notification");
        }
        else
        {
            var result = await CallApi(i => i.CreateTruckAsync(Truck));

            if (!result.Success)
                return;
            
            Toast?.Show("A new truck has been created successfully.", "Notification");
            ResetData();
        }
    }

    private void ResetData()
    {
        Truck.TruckNumber = null;
        Truck.DriverId = null;
    }

    public async Task<IEnumerable<DataListItem>> SearchUser(string searchInput)
    {
        var result = await CallApi(i => i.GetEmployeesAsync(searchInput));
        var pagedList = result.Value;
        var dataListItems = new List<DataListItem>();

        if (pagedList?.Items != null)
        {
            foreach (var item in pagedList.Items)
            {
                dataListItems.Add(new DataListItem(item.Id!, item.GetFullName()));
            }
        }

        return dataListItems;
    }
}
