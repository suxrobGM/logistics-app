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

    protected async Task<bool> CallApiAsync(Func<IApiClient, Task<bool>> apiFunction)
    {
        IsLoading = true;

        await TrySetAccessTokenAsync();
        var success = await apiFunction(ApiClient);
        IsLoading = false;
        return success;
    }

    protected async Task<T?> CallApiAsync<T>(Func<IApiClient, Task<T?>> apiFunction)
    {
        IsLoading = true;

        await TrySetAccessTokenAsync();
        var result = await apiFunction(ApiClient);
        IsLoading = false;
        return result;
    }

    protected async Task<PagedData<T>?> CallApiAsync<T>(Func<IApiClient, Task<PagedResponse<T>?>> apiFunction)
    {
        IsLoading = true;

        await TrySetAccessTokenAsync();
        var result = await apiFunction(ApiClient);
        IsLoading = false;
        return result != null
            ? new PagedData<T>(result.Items, result.Pagination.Total) : null;
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
