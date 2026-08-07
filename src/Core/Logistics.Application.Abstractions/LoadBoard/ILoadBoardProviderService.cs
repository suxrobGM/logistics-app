using Logistics.Domain.Entities;
using Logistics.Domain.Primitives.Enums;
using Logistics.Shared.Models;
using Logistics.Application.Abstractions.LoadBoard;

namespace Logistics.Application.Abstractions.LoadBoard;

/// <summary>
/// Interface for load board provider integrations.
/// Each load board provider (DAT, Truckstop, 123Loadboard) implements this interface.
/// </summary>
public interface ILoadBoardProviderService
{
    /// <summary>
    /// The type of load board provider this service handles
    /// </summary>
    LoadBoardProviderType ProviderType { get; }

    /// <summary>
    /// Initialize the service with credentials from the configuration
    /// </summary>
    void Initialize(LoadBoardConfiguration configuration);

    /// <summary>
    /// Whether the provider authenticates API calls with an OAuth access token that must be
    /// acquired and persisted on the tenant configuration before <see cref="Initialize"/> is useful.
    /// False for providers that send the API key directly on each request.
    /// </summary>
    bool RequiresOAuthToken { get; }

    /// <summary>
    /// Validate that the provided credentials are correct
    /// </summary>
    Task<bool> ValidateCredentialsAsync(string apiKey, string? apiSecret);

    /// <summary>
    /// Acquire a new OAuth access token from the stored credentials.
    /// Returns null when the provider does not use OAuth tokens or acquisition fails.
    /// </summary>
    Task<OAuthTokenResultDto?> AcquireTokenAsync(string apiKey, string? apiSecret);

    /// <summary>
    /// Refresh an OAuth access token using a refresh token
    /// </summary>
    Task<OAuthTokenResultDto?> RefreshTokenAsync(string refreshToken);

    /// <summary>
    /// Search for available loads on the load board
    /// </summary>
    Task<IEnumerable<LoadBoardListingDto>> SearchLoadsAsync(LoadBoardSearchCriteria criteria);

    /// <summary>
    /// Get detailed information about a specific load listing
    /// </summary>
    Task<LoadBoardListingDto?> GetLoadDetailsAsync(string externalListingId);

    /// <summary>
    /// Book/claim a load from the load board
    /// </summary>
    Task<LoadBoardBookingResultDto> BookLoadAsync(string externalListingId, LoadBoardBookingRequest request);

    /// <summary>
    /// Cancel a booking on the load board
    /// </summary>
    Task<bool> CancelBookingAsync(string externalListingId, string? reason);

    /// <summary>
    /// Post a truck to the load board to advertise available capacity
    /// </summary>
    Task<PostTruckResultDto> PostTruckAsync(PostTruckRequest request);

    /// <summary>
    /// Update an existing truck post on the load board
    /// </summary>
    Task<bool> UpdateTruckPostAsync(string externalPostId, PostTruckRequest request);

    /// <summary>
    /// Remove a truck post from the load board
    /// </summary>
    Task<bool> RemoveTruckPostAsync(string externalPostId);

    /// <summary>
    /// Get all posted trucks for the current account
    /// </summary>
    Task<IEnumerable<PostedTruckDto>> GetPostedTrucksAsync();

    /// <summary>
    /// Process an incoming webhook from the load board provider
    /// </summary>
    Task<LoadBoardWebhookResultDto> ProcessWebhookAsync(string payload, string? signature, string? webhookSecret);
}
