using Logistics.WebApi.Client.Exceptions;

namespace Logistics.AdminApp.ViewModels.Pages;

public abstract class PageViewModelBase : ViewModelBase
{
    private readonly IApiClient _apiClient;

    protected PageViewModelBase(IApiClient apiClient)
    {
        _apiClient = apiClient;
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
    
    protected async Task<Result<T>> CallApi<T>(Func<IApiClient, Task<T>> action, bool showBusyIndicator = true) where T : class
    {
        try
        {
            IsBusy = showBusyIndicator;
            var actionResult = await action(_apiClient);
            return new Result<T>(actionResult, true);
        }
        catch (ApiException e)
        {
            Error = e.Message;
            return new Result<T>(null, false);
        }
        finally
        {
            IsBusy = false;
        }
    }
    
    protected async Task<Result> CallApi(Func<IApiClient, Task> action, bool showBusyIndicator = true)
    {
        return await CallApi(async i =>
        {
            await action(i);
            return string.Empty;
        }, showBusyIndicator);
    }
}

public record Result(bool Success);
public record Result<T>(T? Value, bool Success) : Result(Success) where T : class;
