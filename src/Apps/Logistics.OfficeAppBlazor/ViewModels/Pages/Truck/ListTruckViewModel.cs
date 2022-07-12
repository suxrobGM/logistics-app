namespace Logistics.OfficeApp.ViewModels.Pages.Truck;

public class ListTruckViewModel : PageViewModelBase
{
    public ListTruckViewModel(IApiClient apiClient)
        : base(apiClient)
    {
        _trucks = new List<TruckDto>();
    }


    #region Binding properties

    private IList<TruckDto> _trucks;
    public IList<TruckDto> Trucks
    {
        get => _trucks;
        set => SetProperty(ref _trucks, value);
    }

    private int _totalRecords;
    public int TotalRecords
    {
        get => _totalRecords;
        set => SetProperty(ref _totalRecords, value);
    }

    #endregion

    
    public override async Task OnInitializedAsync()
    {
        await base.OnInitializedAsync();
        
        var result = await CallApi(i => i.GetTrucksAsync(page: 1, includeCargoIds: true));

        if (!result.Success)
            return;

        var pagedList = result.Value;
        if (pagedList?.Items != null)
        {
            Trucks = pagedList.Items;
            TotalRecords = pagedList.TotalItems;
        }
    }
}
