namespace Logistics.AdminApp.Pages;

public abstract class PageBase : ComponentBase
{
    #region Injectable services

    [Inject]
    protected IApiClient ApiClient { get; set; } = default!;

    #endregion
    
    
    #region Parameters

    [CascadingParameter]
    public Toast? Toast { get; set; }
    
    [CascadingParameter]
    public Alert? Alert { get; set; }

    #endregion


    #region Bindable properties

    private bool _isBusy;

    protected bool IsBusy
    {
        get => _isBusy;
        set => SetValue(ref _isBusy, value);
    }

    private string _busyText = string.Empty;
    protected string BusyText
    {
        get => _busyText;
        set => SetValue(ref _busyText, value );
    }

    private string _error = string.Empty;
    protected string Error
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

    protected void SetValue<T>(ref T storage, T value)
    {
        if (storage == null || storage.Equals(value)) 
            return;
        
        storage = value;
        StateHasChanged();
    }
}