namespace Logistics.AdminApp.ViewModels.Pages;

public class PageViewModelBase : ViewModelBase
{
    protected readonly IApiClient apiClient;

    public PageViewModelBase(IApiClient apiClient)
    {
        this.apiClient = apiClient;
    }

    private bool _isBusy;
    public bool IsBusy
    {
        get => _isBusy;
        set => SetProperty(ref _isBusy, value);
    }
}
