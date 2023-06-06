using System.Net.Http.Json;
using Logistics.Application.Common.Options;

namespace Logistics.Application.Common.Services;

internal sealed class GoogleRecaptchaService : ICaptchaService
{
    private const string ApiEndpoint = "https://www.google.com/recaptcha/api/siteverify";
    private readonly GoogleRecaptchaOptions _options;
    private readonly HttpClient _httpClient;

    public GoogleRecaptchaService(GoogleRecaptchaOptions options)
    {
        if (string.IsNullOrEmpty(options.SecretKey))
            throw new ArgumentException("Secret key is an empty string");

        _options = options;
        _httpClient = new HttpClient();
    }

    public async Task<bool> VerifyCaptchaAsync(string captchaValue)
    {
        var postQueries = new List<KeyValuePair<string, string>>
        {
            new("secret", _options.SecretKey!),
            new("response", captchaValue)
        };

        var response = await _httpClient.PostAsync(new Uri(ApiEndpoint), new FormUrlEncodedContent(postQueries));
        var recaptchaResponse = await response.Content.ReadFromJsonAsync<GoogleRecaptchaResponse>();
        return recaptchaResponse?.Success ?? false;
    }
}

// ReSharper disable once ClassNeverInstantiated.Global
internal record GoogleRecaptchaResponse
{
    public bool Success { get; init; }
    public float Score { get; init; }
    public string? Action { get; init; }
    public string? HostName { get; init; }
}