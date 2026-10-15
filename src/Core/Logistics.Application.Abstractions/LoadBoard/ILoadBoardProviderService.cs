using Logistics.Domain.Entities;
using Logistics.Domain.Primitives.Enums;
using Logistics.Shared.Models;

namespace Logistics.Application.Abstractions.LoadBoard;

/// <summary>
/// A load board vendor integration. Every call except <see cref="ValidateCredentialsAsync"/>
/// throws <see cref="LoadBoardApiException"/> on a vendor failure; callers go through the
/// application's load board gateway, which handles tokens, the one 401 retry, and turns the
/// exception into a per-provider error.
/// </summary>
/// <remarks>
/// There is no booking call: no supported board exposes a booking API, so booking creates the
/// load in the TMS and the dispatcher confirms with the broker by phone or email.
/// </remarks>
public interface ILoadBoardProviderService
{
    LoadBoardProviderType ProviderType { get; }

    /// <summary>
    /// True when API calls need an access token stored on the tenant configuration;
    /// false when the API key itself is sent on each request.
    /// </summary>
    bool RequiresOAuthToken { get; }

    /// <summary>
    /// Binds the service to a tenant configuration. Tokens are read from it at send time, so a
    /// token the gateway renews on the same configuration is picked up without re-initializing.
    /// </summary>
    void Initialize(LoadBoardConfiguration configuration);

    /// <summary>
    /// Checks the credentials of a provider that does not use tokens. Token providers prove
    /// their credentials by acquiring a token instead.
    /// </summary>
    Task<bool> ValidateCredentialsAsync(LoadBoardConfiguration configuration, CancellationToken ct = default);

    /// <summary>
    /// Acquires a fresh access token from the stored credentials.
    /// </summary>
    Task<OAuthTokenResultDto> AcquireTokenAsync(LoadBoardConfiguration configuration, CancellationToken ct = default);

    /// <summary>
    /// Renews the access token from the stored refresh credential (<see cref="LoadBoardConfiguration.RefreshToken"/>);
    /// null when the provider has none or it was rejected, and the caller falls back to
    /// <see cref="AcquireTokenAsync"/>.
    /// </summary>
    Task<OAuthTokenResultDto?> RefreshTokenAsync(LoadBoardConfiguration configuration, CancellationToken ct = default);

    Task<IReadOnlyList<LoadBoardListingDto>> SearchLoadsAsync(LoadBoardSearchCriteria criteria, CancellationToken ct = default);

    /// <summary>
    /// Posts a truck to advertise capacity and returns the board's id for the post.
    /// </summary>
    Task<PostTruckResultDto> PostTruckAsync(PostTruckRequest request, CancellationToken ct = default);

    Task UpdateTruckPostAsync(string externalPostId, PostTruckRequest request, CancellationToken ct = default);

    Task RemoveTruckPostAsync(string externalPostId, CancellationToken ct = default);

    /// <summary>
    /// The truck posts the board currently holds for this account, used to reconcile local
    /// posts that expired or were removed on the board; null when the provider keeps no posts of
    /// its own to compare against (Demo).
    /// </summary>
    Task<IReadOnlyList<ProviderTruckPost>?> GetPostedTrucksAsync(CancellationToken ct = default);

    /// <summary>
    /// Current market rates for the lanes, aligned by index; an entry is null where the board
    /// has no rate. Only called when the tenant enabled market rates on the configuration
    /// (DAT RateView is a separate subscription).
    /// </summary>
    Task<IReadOnlyList<MarketRate?>> GetMarketRatesAsync(IReadOnlyList<MarketRateLane> lanes, CancellationToken ct = default);
}
