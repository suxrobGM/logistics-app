using System.Net.Http.Json;
using System.Text.Json;
using Logistics.Infrastructure.Integrations.Eld.Common;
using Microsoft.Extensions.Logging;

namespace Logistics.Infrastructure.Integrations.Eld.Providers.Geotab;

/// <summary>
/// Thin wrapper around the MyGeotab JSON-RPC endpoint. All MyGeotab calls are
/// POST /apiv1 with a { method, params } body and a { result } / { error } reply.
/// Authentication returns a federated server path that subsequent calls must use.
/// </summary>
internal class GeotabClient(HttpClient httpClient, ILogger<GeotabClient> logger)
{
    private string baseUrl = "https://my.geotab.com";
    private GeotabCredentials? credentials;

    public void SetBaseUrl(string url)
    {
        baseUrl = url.TrimEnd('/');
    }

    public bool IsAuthenticated => credentials is not null;

    public async Task<bool> AuthenticateAsync(string database, string userName, string password, CancellationToken ct = default)
    {
        var auth = await CallAsync<GeotabAuthenticateResult>(
            "Authenticate",
            new { database, userName, password },
            requireAuth: false,
            ct);

        if (auth?.Credentials is null)
        {
            return false;
        }

        credentials = auth.Credentials;
        if (!string.IsNullOrEmpty(auth.Path) && auth.Path != "ThisServer")
        {
            baseUrl = $"https://{auth.Path}";
        }

        return true;
    }

    public Task<List<T>?> GetAsync<T>(string typeName, object? search = null, CancellationToken ct = default)
    {
        return CallAsync<List<T>>(
            "Get",
            new { typeName, search },
            requireAuth: true,
            ct);
    }

    private async Task<T?> CallAsync<T>(string method, object @params, bool requireAuth, CancellationToken ct)
    {
        if (requireAuth && credentials is null)
        {
            throw new InvalidOperationException("Geotab client is not authenticated.");
        }

        var paramsObj = requireAuth
            ? MergeWithCredentials(@params)
            : @params;

        var request = new GeotabRpcRequest(method, paramsObj);

        try
        {
            var response = await httpClient.PostAsJsonAsync($"{baseUrl}/apiv1", request, EldJsonOptions.CamelCase, ct);

            if (!response.IsSuccessStatusCode)
            {
                logger.LogWarning("Geotab {Method} returned {StatusCode}", method, response.StatusCode);
                return default;
            }

            var rpc = await response.Content.ReadFromJsonAsync<GeotabRpcResponse<T>>(EldJsonOptions.CamelCase, ct);
            if (rpc?.Error is not null)
            {
                logger.LogWarning("Geotab {Method} error: {Name} - {Message}", method, rpc.Error.Name, rpc.Error.Message);
                return default;
            }

            return rpc is null ? default : rpc.Result;
        }
        catch (Exception ex) when (ex is HttpRequestException or JsonException or TaskCanceledException)
        {
            logger.LogError(ex, "Geotab {Method} call failed", method);
            return default;
        }
    }

    private object MergeWithCredentials(object @params)
    {
        var paramsJson = JsonSerializer.SerializeToElement(@params, EldJsonOptions.CamelCase);
        var dict = new Dictionary<string, object?>();
        foreach (var prop in paramsJson.EnumerateObject())
        {
            dict[prop.Name] = JsonSerializer.Deserialize<object?>(prop.Value.GetRawText(), EldJsonOptions.CamelCase);
        }
        dict["credentials"] = credentials;
        return dict;
    }
}
