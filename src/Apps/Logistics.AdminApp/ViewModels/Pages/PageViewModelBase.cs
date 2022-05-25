namespace Logistics.AdminApp.ViewModels.Pages;

public class PageViewModelBase : ViewModelBase
{
    protected readonly IApiClient apiClient;

    public PageViewModelBase(IApiClient apiClient)
    {
        this.apiClient = apiClient;
        _busyText = "Loading Data";
    }

    private bool _isBusy;
    public bool IsBusy
    {
        get => _isBusy;
        set => SetProperty(ref _isBusy, value);
    }

    private string _busyText;
    public string BusyText
    {
        get => _busyText;
        set => SetProperty(ref _busyText, value );
    }
}
