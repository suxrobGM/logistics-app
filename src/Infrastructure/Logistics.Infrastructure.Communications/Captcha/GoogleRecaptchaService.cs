using System.Net.Http.Json;
using Logistics.Application.Services;
using Microsoft.Extensions.Options;

namespace Logistics.Infrastructure.Communications.Captcha;

internal sealed class GoogleRecaptchaService : ICaptchaService
{
    private const string ApiEndpoint = "https://www.google.com/recaptcha/api/siteverify";
    private readonly HttpClient httpClient = new();
    private readonly GoogleRecaptchaOptions options;

    public GoogleRecaptchaService(IOptions<GoogleRecaptchaOptions> options)
    {
        if (string.IsNullOrEmpty(options.Value.SecretKey))
        {
            throw new ArgumentException("Secret key is an empty string");
        }

        this.options = options.Value;
    }

    public async Task<bool> VerifyCaptchaAsync(string captchaValue)
    {
        var postQueries = new List<KeyValuePair<string, string>>
        {
            new("secret", options.SecretKey!),
            new("response", captchaValue)
        };

        var response = await httpClient.PostAsync(new Uri(ApiEndpoint), new FormUrlEncodedContent(postQueries));
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
