using Logistics.Application.Abstractions.LoadBoard;
using Logistics.Domain.Entities;
using Logistics.Domain.Primitives.Enums;
using Logistics.Shared.Models;

namespace Logistics.Infrastructure.Integrations.LoadBoard.Providers;

/// <summary>
/// A board we have not integrated yet because its API could not be verified. Every call fails
/// with a plain "not available yet", so a configuration cannot be created and an old one shows
/// that error per provider instead of pretending to search.
/// </summary>
internal abstract class NotYetAvailableLoadBoardService : ILoadBoardProviderService
{
    public abstract LoadBoardProviderType ProviderType { get; }

    public bool RequiresOAuthToken => false;

    public void Initialize(LoadBoardConfiguration configuration)
    {
    }

    public Task<bool> ValidateCredentialsAsync(LoadBoardConfiguration configuration, CancellationToken ct = default) =>
        throw NotAvailable();

    public Task<OAuthTokenResultDto> AcquireTokenAsync(LoadBoardConfiguration configuration, CancellationToken ct = default) =>
        throw NotAvailable();

    public Task<OAuthTokenResultDto?> RefreshTokenAsync(LoadBoardConfiguration configuration, CancellationToken ct = default) =>
        throw NotAvailable();

    public Task<IReadOnlyList<LoadBoardListingDto>> SearchLoadsAsync(LoadBoardSearchCriteria criteria, CancellationToken ct = default) =>
        throw NotAvailable();

    public Task<PostTruckResultDto> PostTruckAsync(PostTruckRequest request, CancellationToken ct = default) =>
        throw NotAvailable();

    public Task UpdateTruckPostAsync(string externalPostId, PostTruckRequest request, CancellationToken ct = default) =>
        throw NotAvailable();

    public Task RemoveTruckPostAsync(string externalPostId, CancellationToken ct = default) =>
        throw NotAvailable();

    public Task<IReadOnlyList<ProviderTruckPost>?> GetPostedTrucksAsync(CancellationToken ct = default) =>
        throw NotAvailable();

    public Task<IReadOnlyList<MarketRate?>> GetMarketRatesAsync(IReadOnlyList<MarketRateLane> lanes, CancellationToken ct = default) =>
        throw NotAvailable();

    private LoadBoardApiException NotAvailable() =>
        new($"The {ProviderType.GetDescription()} integration is not available yet.");
}
