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
        
        var result = await CallApi(i => i.GetCargoesAsync(page: 1));

        if (!result.Success)
            return;

        var pagedList = result.Value;
        if (pagedList?.Items != null)
        {
            Cargoes = pagedList.Items;
            TotalRecords = pagedList.TotalItems;
        }
    }
}
