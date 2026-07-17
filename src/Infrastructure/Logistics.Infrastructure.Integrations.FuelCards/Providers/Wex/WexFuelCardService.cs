using System.Net.Http.Headers;
using Logistics.Application.Abstractions.FuelCards;
using Logistics.Domain.Entities;
using Logistics.Domain.Primitives.Enums;
using Logistics.Infrastructure.Integrations.FuelCards.Common;
using Logistics.Shared.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Logistics.Infrastructure.Integrations.FuelCards.Providers.Wex;

/// <summary>
/// WEX fleet card API client (WEX OTR / ClearView). Requires a partner API key.
/// </summary>
internal class WexFuelCardService(
    HttpClient httpClient,
    IOptions<FuelCardsOptions> options,
    ILogger<WexFuelCardService> logger) : IFuelCardProviderService
{
    private readonly WexOptions options = options.Value.Wex ?? new WexOptions();

    public FuelCardProviderType ProviderType => FuelCardProviderType.Wex;

    public void Initialize(FuelCardProviderConfiguration configuration)
    {
        httpClient.BaseAddress ??= new Uri(options.BaseUrl);
        httpClient.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", configuration.AccessToken ?? configuration.ApiKey);
    }

    public async Task<bool> ValidateCredentialsAsync(string apiKey, string? apiSecret)
    {
        httpClient.BaseAddress ??= new Uri(options.BaseUrl);
        httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);

        var response = await httpClient.TryGetFromJsonAsync<WexTransactionsResponse>(
            "/fleet/v1/transactions?limit=1", logger, "validate credentials");
        return response is not null;
    }

    public Task<OAuthTokenResultDto?> RefreshTokenAsync(string refreshToken)
    {
        // WEX fleet APIs use long-lived API keys, not OAuth refresh flows
        return Task.FromResult<OAuthTokenResultDto?>(null);
    }

    public async Task<IReadOnlyList<FuelCardTransactionData>> GetTransactionsAsync(
        DateTime sinceUtc, CancellationToken ct = default)
    {
        var url = $"/fleet/v1/transactions?startDate={sinceUtc:yyyy-MM-dd}&endDate={DateTime.UtcNow:yyyy-MM-dd}";
        var response = await httpClient.TryGetFromJsonAsync<WexTransactionsResponse>(
            url, logger, "get transactions", ct);

        if (response?.Transactions is null)
        {
            return [];
        }

        return response.Transactions
            .Select(WexMapper.ToTransactionData)
            .Where(t => t is not null)
            .Select(t => t!)
            .ToList();
    }
}
