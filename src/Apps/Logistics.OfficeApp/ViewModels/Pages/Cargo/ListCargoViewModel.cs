namespace Logistics.OfficeApp.ViewModels.Pages.Cargo;

public class ListCargoViewModel : PageViewModelBase
{
    public ListCargoViewModel(IApiClient apiClient)
        : base(apiClient)
    {
        _cargoes = new List<CargoDto>();
    }


    #region Binding properties

    private IList<CargoDto> _cargoes;
    public IList<CargoDto> Cargoes
    {
        get => _cargoes;
        set => SetProperty(ref _cargoes, value);
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
            var pagedList = await apiClient.GetCargoesAsync(page: 1);

            if (pagedList.Items != null)
            {
                Cargoes = pagedList.Items;
                TotalRecords = pagedList.TotalItems;
            }

            IsBusy = false;
        }
        catch (ApiException e)
        {
            Console.WriteLine(e);
            throw;
        }
        finally
        {
            IsBusy = false;
        }
    }
}
