using Logistics.Shared;

namespace Logistics.AdminApp.Pages;

public abstract class PageBase : ComponentBase
{
    #region Injectable services

    [Inject]
    private IApiClient ApiClient { get; set; } = default!;

    #endregion
    
    
    #region Parameters

    [CascadingParameter]
    public Toast? Toast { get; set; }
    
    [CascadingParameter]
    public Alert? Alert { get; set; }

    #endregion


    #region Bindable properties

    private bool _isLoading;

    protected bool IsLoading
    {
        get => _isLoading;
        set => SetValue(ref _isLoading, value);
    }

    private string _spinnerText = string.Empty;
    protected string SpinnerText
    {
        get => _spinnerText;
        set => SetValue(ref _spinnerText, value );
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
    
    protected async Task<bool> CallApiAsync(Func<IApiClient, Task<ResponseResult>> apiFunction)
    {
        Error = string.Empty;
        IsLoading = true;
        
        var apiResult = await apiFunction(ApiClient);
        IsLoading = false;
        return HandleError(apiResult);
    }

    protected async Task<T?> CallApiAsync<T>(Func<IApiClient, Task<ResponseResult<T>>> apiFunction)
    {
        Error = string.Empty;
        IsLoading = true;
        
        var apiResult = await apiFunction(ApiClient);
        HandleError(apiResult);
        IsLoading = false;
        return apiResult.Value;
    }
    
    protected async Task<PagedData<T>?> CallApiAsync<T>(Func<IApiClient, Task<PagedResponseResult<T>>> apiFunction)
    {
        Error = string.Empty;
        IsLoading = true;
        
        var apiResult = await apiFunction(ApiClient);
        HandleError(apiResult);
        IsLoading = false;
        return apiResult.Items != null
            ? new PagedData<T>(apiResult.Items, apiResult.ItemsCount, apiResult.PagesCount) : default;
    }

    private bool HandleError(IResponseResult apiResult)
    {
        if (!apiResult.Success && !string.IsNullOrEmpty(apiResult.Error))
        {
            Error = apiResult.Error;
            return false;
        }

        return true;
    }
}

public record PagedData<T>(IEnumerable<T> Items, int ItemsCount, int PageSize);
