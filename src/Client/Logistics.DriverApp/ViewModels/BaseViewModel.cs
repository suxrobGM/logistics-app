namespace Logistics.DriverApp.ViewModels;

public abstract class BaseViewModel : ObservableRecipient
{
    private bool _isInitialized;
    public event EventHandler<bool>? IsLoadingChanged;

    protected BaseViewModel()
    {
        InitializeCommand = new AsyncRelayCommand(HandleInitializeCommandAsync);
        DisappearingCommand = new AsyncRelayCommand(OnDisappearingAsync);
    }

    #region Commands

    public IAsyncRelayCommand InitializeCommand { get; }
    public IAsyncRelayCommand DisappearingCommand { get; }

    #endregion


    #region Bindable properties

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

    #endregion


    protected virtual Task OnInitializedAsync()
    {
        return Task.CompletedTask;
    }

    protected virtual Task OnAppearingAsync()
    {
        return Task.CompletedTask;
    }

    protected virtual Task OnDisappearingAsync()
    {
        return Task.CompletedTask;
    }

    private async Task HandleInitializeCommandAsync()
    {
        await OnAppearingAsync();

        if (_isInitialized)
            return;

        _isInitialized = true;
        await OnInitializedAsync();
    }
}
