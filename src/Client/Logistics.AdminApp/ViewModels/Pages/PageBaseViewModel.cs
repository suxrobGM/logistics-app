namespace Logistics.AdminApp.ViewModels.Pages;

public abstract class PageBaseViewModel : ViewModelBase
{
    protected PageBaseViewModel()
    {
        _busyText = string.Empty;
        _error = string.Empty;
    }
    
    [CascadingParameter]
    public Toast? Toast { get; set; }
    
    [CascadingParameter]
    public Alert? Alert { get; set; }

    
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
        set
        {
            _error = value;
            if (!string.IsNullOrEmpty(value))
            {
                Alert?.Show(value, Alert.AlertType.Error);
            }
        }
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