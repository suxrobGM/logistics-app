using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Logistics.HttpClient.Exceptions;
using Logistics.HttpClient.Utils;

namespace Logistics.HttpClient.Implementations;

internal class GenericApiClient
{
    private readonly System.Net.Http.HttpClient _httpClient;
    private readonly JsonSerializerOptions _serializerOptions;

    public GenericApiClient(string host)
    {
        if (string.IsNullOrEmpty(host))
            throw new ArgumentNullException(host);

        _serializerOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };

        try
        {
            var handler = new HttpClientHandler
            {
                // Disable SSL validation
                ServerCertificateCustomValidationCallback = (_, _, _, _) => true
            };

            _httpClient = new System.Net.Http.HttpClient(handler) { BaseAddress = new Uri(host) };
        }
        catch (PlatformNotSupportedException)
        {
            _httpClient = new System.Net.Http.HttpClient { BaseAddress = new Uri(host) };
        }
    }

    public void SetAuthorizationHeader(string scheme, string? value)
    {
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(scheme, value);
    }

    public void SetRequestHeader(string key, string? value)
    {
        if (_httpClient.DefaultRequestHeaders.Contains(key))
        {
            _httpClient.DefaultRequestHeaders.Remove(key);
        }
        _httpClient.DefaultRequestHeaders.Add(key, value);
    }

    public async Task<TRes> GetRequestAsync<TRes>(string endpoint,
        IDictionary<string, string>? queries = null)
    {
        var content = await GetRequestAsync(endpoint, queries);
        var deserializedObj = Deserialize<TRes>(content);

        if (deserializedObj is null)
        {
            throw new ApiException(
                $"Serialization error, failed to deserialize object from response content:\n{content}");
        }

        return deserializedObj;
    }

    public async Task<string> GetRequestAsync(string endpoint,
        IDictionary<string, string>? queries = null)
    {
        var requestEndpoint = endpoint;
        if (queries is { Count: > 0 })
            requestEndpoint = QueryUtils.BuildQueryParameters(endpoint, queries);

        try
        {
            using var response = await _httpClient.GetAsync(requestEndpoint);
            var content = await ReadContentAsync(response);
            return content;
        }
        catch (HttpRequestException e)
        {
            throw new ApiException(e.Message);
        }
    }

    public async Task<TRes> PostRequestAsync<TRes, TBody>(string endpoint, TBody body)
    {
        var content = await PostRequestAsync(endpoint, body);
        var deserializedObj = Deserialize<TRes>(content);

        if (deserializedObj is null)
        {
            throw new ApiException(
                $"Serialization error, failed to deserialize object from response content:\n{content}");
        }

        return deserializedObj;
    }

    public async Task<string> PostRequestAsync<TRequest>(string endpoint, TRequest body)
    {
        try
        {
            using var response = await _httpClient.PostAsync(endpoint, GetJsonContent(body));
            var content = await ReadContentAsync(response);
            return content;
        }
        catch (HttpRequestException e)
        {
            throw new ApiException(e.Message);
        }
    }

    public async Task<TRes> PutRequestAsync<TRes, TBody>(string endpoint, TBody body)
    {
        var content = await PutRequestAsync(endpoint, body);
        var deserializedObj = Deserialize<TRes>(content);

        if (deserializedObj is null)
        {
            throw new ApiException(
                $"Serialization error, failed to deserialize object from response content:\n{content}");
        }

        return deserializedObj;
    }

    public async Task<string> PutRequestAsync<TBody>(string endpoint, TBody body)
    {
        try
        {
            using var response = await _httpClient.PutAsync(endpoint, GetJsonContent(body));
            var content = await ReadContentAsync(response);
            return content;
        }
        catch (HttpRequestException e)
        {
            throw new ApiException(e.Message);
        }
    }

    public async Task<TRes> DeleteRequestAsync<TRes>(string endpoint,
        IDictionary<string, string> queries = default!)
        where TRes : new()
    {
        var content = await DeleteRequestAsync(endpoint, queries);
        var deserializedObj = Deserialize<TRes>(content);

        if (deserializedObj is null)
        {
            throw new ApiException(
                $"Serialization error, failed to deserialize object from response content:\n{content}");
        }

        return deserializedObj;
    }

    public async Task<string> DeleteRequestAsync(string endpoint,
        IDictionary<string, string>? queries = null!)
    {
        var requestEndpoint = endpoint;
        if (queries?.Count > 0)
            requestEndpoint = QueryUtils.BuildQueryParameters(endpoint, queries);

        try
        {
            using var response = await _httpClient.DeleteAsync(requestEndpoint);
            var content = await ReadContentAsync(response);
            return content;
        }
        catch (HttpRequestException e)
        {
            throw new ApiException(e.Message);
        }
    }

    private async Task<string> ReadContentAsync(HttpResponseMessage response)
    {
        var content = await response.Content.ReadAsStringAsync();

        if (response.StatusCode == HttpStatusCode.Unauthorized)
        {
            throw new ApiException("Unauthorized request, please check request's access token");
        }

        if (!response.IsSuccessStatusCode)
        {
            var errorMessage = content;
            var errorData = Deserialize<ErrorData>(content);

            if (!string.IsNullOrEmpty(errorData?.Error))
            {
                errorMessage = errorData.Error;
            }

            if (string.IsNullOrEmpty(errorMessage))
            {
                errorMessage = $"Something went wrong, response status code: {(int)response.StatusCode}";
            }

            throw new ApiException(errorMessage);
        }

        return content;
    }

    private HttpContent GetJsonContent<T>(T data)
    {
        var jsonData = JsonSerializer.Serialize(data, _serializerOptions);
        return new StringContent(jsonData, Encoding.UTF8, "application/json");
    }

    private T? Deserialize<T>(string json)
    {
        try
        {
            return JsonSerializer.Deserialize<T>(json, _serializerOptions);
        }
        catch (JsonException)
        {
            return default;
        }
    }
}
