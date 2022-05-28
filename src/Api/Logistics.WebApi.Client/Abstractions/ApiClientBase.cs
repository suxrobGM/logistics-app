using System.Text;
using System.Text.Json;

namespace Logistics.WebApi.Client;

internal abstract class ApiClientBase
{
    private readonly HttpClient _httpClient;
    private readonly JsonSerializerOptions _serializerOptions;

    protected ApiClientBase(string host)
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

            _httpClient = new HttpClient(handler) { BaseAddress = new Uri(host) };
        }
        catch (PlatformNotSupportedException)
        {
            _httpClient = new HttpClient { BaseAddress = new Uri(host) };
        }
    }

    protected void SetRequestHeader(string key, string? value)
    {
        if (_httpClient.DefaultRequestHeaders.Contains(key))
        {
            _httpClient.DefaultRequestHeaders.Remove(key);
        }
        _httpClient.DefaultRequestHeaders.Add(key, value);
    }

    protected async Task<TResponse> GetRequestAsync<TResponse>(string endpoint,
        IDictionary<string, string> queries = default!)
        where TResponse : new()
    {
        var content = await GetRequestAsync(endpoint, queries);
        var deserializedObj = Deserialize<TResponse>(content) ?? default;

        if (deserializedObj is null)
        {
            throw new ApiException(
                $"Serialization error, failed to deserialize object from response content:\n{content}");
        }

        return deserializedObj;
    }

    protected async Task<string> GetRequestAsync(string endpoint,
        IDictionary<string, string> queries = default!)
    {
        var requestEndpoint = endpoint;
        if (queries != null && queries.Count > 0)
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

    protected async Task<TResponse> PostRequestAsync<TResponse, TRequest>(string endpoint, TRequest body)
        where TResponse : new()
        where TRequest : new()
    {
        var content = await PostRequestAsync(endpoint, body);
        var deserializedObj = Deserialize<TResponse>(content) ?? default(TResponse);

        if (deserializedObj is null)
        {
            throw new ApiException(
                $"Serialization error, failed to deserialize object from response content:\n{content}");
        }

        return deserializedObj;
    }

    protected async Task<string> PostRequestAsync<TRequest>(string endpoint, TRequest body)
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

    protected async Task<TResponse> PutRequestAsync<TResponse, TRequest>(string endpoint, TRequest body)
        where TResponse : new()
        where TRequest : new()
    {
        var content = await PutRequestAsync(endpoint, body);
        var deserializedObj = Deserialize<TResponse>(content) ?? default(TResponse);

        if (deserializedObj is null)
        {
            throw new ApiException(
                $"Serialization error, failed to deserialize object from response content:\n{content}");
        }

        return deserializedObj;
    }

    protected async Task<string> PutRequestAsync<TRequest>(string endpoint, TRequest body)
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

    protected async Task<TResponse> DeleteRequestAsync<TResponse>(string endpoint,
        IDictionary<string, string> queries = default!)
        where TResponse : new()
    {
        var content = await DeleteRequestAsync(endpoint, queries);
        var deserializedObj = Deserialize<TResponse>(content) ?? default;

        if (deserializedObj is null)
        {
            throw new ApiException(
                $"Serialization error, failed to deserialize object from response content:\n{content}");
        }

        return deserializedObj;
    }

    protected async Task<string> DeleteRequestAsync(string endpoint,
        IDictionary<string, string> queries = default!)
    {
        var requestEndpoint = endpoint;
        if (queries.Count > 0)
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

        if (!response.IsSuccessStatusCode)
        {
            var errorMessage = content;
            var errorData = Deserialize<ErrorData>(content);

            if (errorData != null && !string.IsNullOrEmpty(errorData.Error))
            {
                errorMessage = errorData.Error;
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
