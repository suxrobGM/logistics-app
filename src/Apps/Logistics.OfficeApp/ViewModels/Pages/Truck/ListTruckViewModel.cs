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
        
        try
        {
            IsBusy = true;
            var pagedList = await apiClient.GetTrucksAsync(page: 1, includeCargoIds: true);
            if (pagedList.Items != null)
            {
                Trucks = pagedList.Items;
                TotalRecords = pagedList.TotalItems;
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
}
