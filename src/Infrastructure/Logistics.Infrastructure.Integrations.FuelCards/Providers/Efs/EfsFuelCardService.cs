using System.Net.Http.Headers;
using Logistics.Application.Abstractions.FuelCards;
using Logistics.Domain.Entities;
using Logistics.Domain.Primitives.Enums;
using Logistics.Infrastructure.Integrations.FuelCards.Common;
using Logistics.Shared.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Logistics.Infrastructure.Integrations.FuelCards.Providers.Efs;

/// <summary>
/// EFS (WEX-owned OTR card program) API client. The card program and API surface are
/// distinct from WEX fleet cards, hence a separate provider.
/// </summary>
internal class EfsFuelCardService(
    HttpClient httpClient,
    IOptions<EfsOptions> options,
    ILogger<EfsFuelCardService> logger) : IFuelCardProviderService
{
    public FuelCardProviderType ProviderType => FuelCardProviderType.Efs;

    public void Initialize(FuelCardProviderConfiguration configuration)
    {
        httpClient.BaseAddress ??= new Uri(options.Value.BaseUrl);
        httpClient.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", configuration.AccessToken ?? configuration.ApiKey);
    }

    public async Task<bool> ValidateCredentialsAsync(string apiKey, string? apiSecret)
    {
        httpClient.BaseAddress ??= new Uri(options.Value.BaseUrl);
        httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);

        var response = await httpClient.TryGetFromJsonAsync<EfsTransactionsResponse>(
            "/v1/transactions?limit=1", logger, "validate credentials");
        return response is not null;
    }

    public Task<OAuthTokenResultDto?> RefreshTokenAsync(string refreshToken)
    {
        return Task.FromResult<OAuthTokenResultDto?>(null);
    }

    public async Task<IReadOnlyList<FuelCardTransactionData>> GetTransactionsAsync(
        DateTime sinceUtc, CancellationToken ct = default)
    {
        var url = $"/v1/transactions?postedAfter={sinceUtc:O}";
        var response = await httpClient.TryGetFromJsonAsync<EfsTransactionsResponse>(
            url, logger, "get transactions", ct);

        if (response?.Data is null)
        {
            return [];
        }

        return response.Data
            .Select(EfsMapper.ToTransactionData)
            .Where(t => t is not null)
            .Select(t => t!)
            .ToList();
    }
}
