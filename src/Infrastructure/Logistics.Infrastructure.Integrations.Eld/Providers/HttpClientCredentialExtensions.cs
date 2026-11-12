using Microsoft.Extensions.Logging;

namespace Logistics.Infrastructure.Integrations.Eld.Providers;

internal static class HttpClientCredentialExtensions
{
    extension(HttpClient httpClient)
    {
        /// <summary>A transport failure is logged and reported as invalid credentials.</summary>
        public async Task<bool> ValidateCredentialsAsync(HttpRequestMessage request, ILogger logger, string providerName)
        {
            try
            {
                using var response = await httpClient.SendAsync(request);
                return response.IsSuccessStatusCode;
            }
            catch (HttpRequestException ex)
            {
                logger.LogError(ex, "Failed to validate {Provider} credentials", providerName);
                return false;
            }
        }
    }
}
