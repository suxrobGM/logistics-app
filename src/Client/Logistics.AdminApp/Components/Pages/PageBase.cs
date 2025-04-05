using Logistics.HttpClient;
using Logistics.Shared.Models;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.WebAssembly.Authentication;
using Radzen;

namespace Logistics.AdminApp.Components.Pages;

public abstract class PageBase : ComponentBase
{
    #region Injectable services

    [Inject]
    private IApiClient ApiClient { get; set; } = null!;

    [Inject] 
    private IAccessTokenProvider AccessTokenProvider { get; set; } = null!;

    [Inject] 
    private NotificationService NotificationService { get; set; } = null!;
    
    [Inject]
    private TooltipService TooltipService { get; set; } = null!;

    #endregion


    #region Bindable properties

    private bool _isLoading;

    protected bool IsLoading
    {
        get => _isLoading;
        set => SetValue(ref _isLoading, value);
    }

    #endregion

    protected void SetValue<T>(ref T storage, T value)
    {
        if (storage is null || storage.Equals(value))
        {
            return;
        }
        
        storage = value;
        StateHasChanged();
    }

    protected void ShowNotification(string message)
    {
        NotificationService.Notify(new NotificationMessage
        {
            Summary = "Notification",
            Detail = message,
            Severity = NotificationSeverity.Success
        });
    }

    protected void ShowTooltip(ElementReference element, string message, TooltipOptions options = null!)
    {
        TooltipService.Open(element, message, options);
    }
    
    protected async Task<bool> CallApiAsync(Func<IApiClient, Task<Result>> apiFunction)
    {
        IsLoading = true;
        
        await TrySetAccessTokenAsync();
        var apiResult = await apiFunction(ApiClient);
        IsLoading = false;
        return HandleError(apiResult);
    }

    protected async Task<T?> CallApiAsync<T>(Func<IApiClient, Task<Result<T>>> apiFunction)
    {
        IsLoading = true;
        
        await TrySetAccessTokenAsync();
        var apiResult = await apiFunction(ApiClient);
        HandleError(apiResult);
        IsLoading = false;
        return apiResult.Data;
    }
    
    protected async Task<PagedData<T>?> CallApiAsync<T>(Func<IApiClient, Task<PagedResult<T>>> apiFunction)
    {
        IsLoading = true;

        await TrySetAccessTokenAsync();
        var apiResult = await apiFunction(ApiClient);
        HandleError(apiResult);
        IsLoading = false;
        return apiResult.Data != null
            ? new PagedData<T>(apiResult.Data, apiResult.TotalItems) : null;
    }

    private bool HandleError(IResult apiResult)
    {
        if (!apiResult.Success && !string.IsNullOrEmpty(apiResult.Error))
        {
            NotificationService.Notify(new NotificationMessage
            {
                Summary = "API Error",
                Detail = apiResult.Error,
                Severity = NotificationSeverity.Error
            });
            Console.Error.WriteLine(apiResult.Error);
            return false;
        }

        return true;
    }

    private async Task TrySetAccessTokenAsync()
    {
        if (!string.IsNullOrEmpty(ApiClient.AccessToken))
        {
            return;
        }

        var result = await AccessTokenProvider.RequestAccessToken();

        if (result.TryGetToken(out var accessToken))
        {
            ApiClient.AccessToken = accessToken.Value;
        }
    }
}

public record PagedData<T>(IEnumerable<T> Items, int TotalItems);
