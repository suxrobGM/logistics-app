namespace Logistics.AdminApp.ViewModels.Pages.Cargo;

public class ListCargoViewModel : PageViewModelBase
{

    public ListCargoViewModel(IApiClient apiClient)
        : base(apiClient)
    {
        Cargoes = new PagedDataResult<CargoDto>();
    }

    private PagedDataResult<CargoDto>? _cargoes;
    public PagedDataResult<CargoDto>? Cargoes 
    {
        get => _cargoes;
        set => SetProperty(ref _cargoes, value);
    }


}
