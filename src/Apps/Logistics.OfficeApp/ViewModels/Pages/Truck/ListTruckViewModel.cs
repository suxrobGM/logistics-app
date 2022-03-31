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
        IsBusy = true;
        var pagedList = await FetchTrucksAsync();
        if (pagedList != null && pagedList.Items != null)
        {
            Trucks = pagedList.Items;
            TotalRecords = pagedList.TotalItems;
        }
        IsBusy = false;
    }

    private Task<PagedDataResult<TruckDto>> FetchTrucksAsync(int page = 1)
    {
        return Task.Run(async () =>
        {
            return await apiClient.GetTrucksAsync(page: page, includeCargoIds: true);
        });
    }
}
