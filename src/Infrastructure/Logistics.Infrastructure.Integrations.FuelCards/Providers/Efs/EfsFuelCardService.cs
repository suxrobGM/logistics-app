using Logistics.Application.Abstractions.FuelCards;
using Logistics.Domain.Primitives.Enums;
using Logistics.Infrastructure.Integrations.Common;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Logistics.Infrastructure.Integrations.FuelCards.Providers.Efs;

/// <summary>
/// EFS (WEX-owned OTR card program) API client. The card program and API surface are
/// distinct from WEX fleet cards, hence a separate provider.
/// </summary>
internal sealed class EfsFuelCardService(
    HttpClient httpClient,
    IOptions<FuelCardsOptions> options,
    ILogger<EfsFuelCardService> logger)
    : BearerFuelCardService<EfsTransactionsResponse>(httpClient, logger)
{
    private readonly EfsOptions options = options.Value.Efs ?? new EfsOptions();

    public override FuelCardProviderType ProviderType => FuelCardProviderType.Efs;

    protected override string BaseUrl => options.BaseUrl;

    protected override string ProbeUrl => "/v1/transactions?limit=1";

    public override async Task<IReadOnlyList<FuelCardTransactionData>> GetTransactionsAsync(
        DateTime sinceUtc, CancellationToken ct = default)
    {
        var url = $"/v1/transactions?postedAfter={sinceUtc:O}";
        var response = await HttpClient.TryGetFromJsonAsync<EfsTransactionsResponse>(
            url, Logger, "get transactions", ct: ct);

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
