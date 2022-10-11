namespace Logistics.DriverApp.ViewModels;

public abstract class ViewModelBase : ObservableRecipient
{
    public event EventHandler<bool>? IsBusyChanged;

    private bool _isBusy = false;
    public bool IsBusy
    {
        get => _isBusy;
        set 
        {
            SetProperty(ref _isBusy, value);
            IsBusyChanged?.Invoke(this, _isBusy);
        }
    }
}
