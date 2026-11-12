using System.Net.Http.Headers;
using Logistics.Application.Abstractions.FuelCards;
using Logistics.Domain.Entities;
using Logistics.Domain.Primitives.Enums;
using Logistics.Infrastructure.Integrations.Common;
using Logistics.Shared.Models;
using Microsoft.Extensions.Logging;

namespace Logistics.Infrastructure.Integrations.FuelCards.Providers;

/// <summary>
/// Shared plumbing for fuel card providers that authenticate with a long-lived bearer API key.
/// Subclasses supply the base URL, a credential-probe URL, and the transaction fetch/mapping -
/// the parts that genuinely differ between providers.
/// </summary>
/// <typeparam name="TProbeResponse">
/// The response shape of <see cref="ProbeUrl"/>. Only used to prove the credentials deserialize.
/// </typeparam>
internal abstract class BearerFuelCardService<TProbeResponse>(HttpClient httpClient, ILogger logger)
    : IFuelCardProviderService
{
    protected HttpClient HttpClient { get; } = httpClient;
    protected ILogger Logger { get; } = logger;

    public abstract FuelCardProviderType ProviderType { get; }

    /// <summary>Provider API root, from configuration.</summary>
    protected abstract string BaseUrl { get; }

    /// <summary>A cheap authenticated GET used to validate credentials.</summary>
    protected abstract string ProbeUrl { get; }

    public void Initialize(FuelCardProviderConfiguration configuration)
    {
        HttpClient.BaseAddress ??= new Uri(BaseUrl);
        HttpClient.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", configuration.AccessToken ?? configuration.ApiKey);
    }

    public async Task<bool> ValidateCredentialsAsync(string apiKey, string? apiSecret)
    {
        HttpClient.BaseAddress ??= new Uri(BaseUrl);
        HttpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);

        var response = await HttpClient.TryGetFromJsonAsync<TProbeResponse>(
            ProbeUrl, Logger, "validate credentials");
        return response is not null;
    }

    /// <summary>
    /// WEX and EFS fleet APIs issue long-lived API keys rather than OAuth refresh flows, so there
    /// is nothing to refresh. A provider that does use OAuth should override this.
    /// </summary>
    public virtual Task<OAuthTokenResultDto?> RefreshTokenAsync(string refreshToken) =>
        Task.FromResult<OAuthTokenResultDto?>(null);

    public abstract Task<IReadOnlyList<FuelCardTransactionData>> GetTransactionsAsync(
        DateTime sinceUtc, CancellationToken ct = default);
}
