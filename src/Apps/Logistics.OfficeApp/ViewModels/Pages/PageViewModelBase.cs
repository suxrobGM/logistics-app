namespace Logistics.OfficeApp.ViewModels.Pages;

public abstract class PageViewModelBase : ViewModelBase
{
    protected readonly IApiClient apiClient;

    protected PageViewModelBase(IApiClient apiClient)
    {
        this.apiClient = apiClient;
        _busyText = string.Empty;
        _error = string.Empty;
    }
    
    [CascadingParameter]
    public Toast? Toast { get; set; }

    
    #region Bindable properties

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

    private string _error;
    public string Error
    {
        get => _error;
        set => SetProperty(ref _error, value);
    }

    #endregion
    

    public override void OnInitialized()
    {
        Error = string.Empty;
    }

    public override Task OnInitializedAsync()
    {
        OnInitialized();
        return Task.CompletedTask;
    }
}
