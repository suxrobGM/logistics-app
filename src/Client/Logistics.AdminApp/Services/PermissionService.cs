using Logistics.HttpClient;
using Microsoft.AspNetCore.Components.WebAssembly.Authentication;

namespace Logistics.AdminApp.Services;

public class PermissionService(IApiClient apiClient, IAccessTokenProvider accessTokenProvider)
{
    private bool _loaded;
    private string[] _permissions = [];

    public async Task LoadPermissionsAsync()
    {
        if (_loaded)
        {
            return;
        }

        await EnsureAccessTokenAsync();
        var permissions = await apiClient.GetCurrentUserPermissionsAsync();
        _permissions = permissions ?? [];
        _loaded = true;
    }

    public bool HasPermission(string permission)
    {
        return _permissions.Contains(permission);
    }

    public void ClearPermissions()
    {
        _permissions = [];
        _loaded = false;
    }

    private async Task EnsureAccessTokenAsync()
    {
        if (!string.IsNullOrEmpty(apiClient.AccessToken))
        {
            return;
        }

        var result = await accessTokenProvider.RequestAccessToken();

        if (result.TryGetToken(out var accessToken))
        {
            apiClient.AccessToken = accessToken.Value;
        }
    }
}
