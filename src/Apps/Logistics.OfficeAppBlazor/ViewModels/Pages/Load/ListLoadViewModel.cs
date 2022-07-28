namespace Logistics.OfficeApp.ViewModels.Pages.Load;

public class ListLoadViewModel : PageViewModelBase
{
    public ListLoadViewModel(IApiClient apiClient)
        : base(apiClient)
    {
        _cargoes = new List<LoadDto>();
    }


    #region Binding properties

    private IList<LoadDto> _cargoes;
    public IList<LoadDto> Cargoes
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
        
        var result = await CallApi(i => i.GetLoadsAsync(page: 1));

        if (!result.Success)
            return;

        var pagedList = result.Value;
        if (pagedList?.Items != null)
        {
            Cargoes = pagedList.Items;
            TotalRecords = pagedList.ItemsCount;
        }
    }
}
