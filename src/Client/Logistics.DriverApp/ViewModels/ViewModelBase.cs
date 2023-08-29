namespace Logistics.DriverApp.ViewModels;

public abstract class ViewModelBase : ObservableRecipient
{
    public event EventHandler<bool>? IsLoadingChanged;

    private bool _isLoading;
    public bool IsLoading
    {
        get => _isLoading;
        set 
        {
            SetProperty(ref _isLoading, value);
            IsLoadingChanged?.Invoke(this, _isLoading);
        }
    }
}
