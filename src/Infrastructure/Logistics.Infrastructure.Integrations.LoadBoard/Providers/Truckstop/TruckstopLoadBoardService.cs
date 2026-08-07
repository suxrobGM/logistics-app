using System.Net.Http.Headers;
using Logistics.Infrastructure.Integrations.Common;
using System.Net.Http.Json;
using System.Text.Json;
using Logistics.Domain.Entities;
using Logistics.Domain.Primitives.Enums;
using Logistics.Shared.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Logistics.Application.Abstractions.LoadBoard;

namespace Logistics.Infrastructure.Integrations.LoadBoard.Providers.Truckstop;

/// <summary>
///     Truckstop.com Load Board provider implementation.
///     Authentication: OAuth 2.0 (Resource Owner Password grant)
///     Access Token validity: 20 minutes, Refresh Token validity: 6 months
/// </summary>
internal class TruckstopLoadBoardService(
    HttpClient httpClient,
    IHttpClientFactory httpClientFactory,
    IOptions<LoadBoardOptions> options,
    ILogger<TruckstopLoadBoardService> logger)
    : ILoadBoardProviderService
{
    private readonly TruckstopOptions options = options.Value.Truckstop ?? new TruckstopOptions();

    public LoadBoardProviderType ProviderType => LoadBoardProviderType.Truckstop;

    public bool RequiresOAuthToken => true;

    public void Initialize(LoadBoardConfiguration configuration)
    {
        httpClient.BaseAddress = new Uri(options.BaseUrl);

        if (!string.IsNullOrEmpty(configuration.AccessToken))
        {
            httpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", configuration.AccessToken);
        }

        logger.LogInformation("Initialized Truckstop Load Board provider");
    }

    public async Task<bool> ValidateCredentialsAsync(string apiKey, string? apiSecret)
    {
        return await AcquireTokenAsync(apiKey, apiSecret) != null;
    }

    public async Task<OAuthTokenResultDto?> AcquireTokenAsync(string apiKey, string? apiSecret)
    {
        return ToTokenResult(await RequestTokenAsync("acquisition", new Dictionary<string, string>
        {
            ["grant_type"] = "password", ["username"] = apiKey, ["password"] = apiSecret ?? string.Empty
        }));
    }

    public async Task<OAuthTokenResultDto?> RefreshTokenAsync(string refreshToken)
    {
        return ToTokenResult(await RequestTokenAsync("refresh", new Dictionary<string, string>
        {
            ["grant_type"] = "refresh_token", ["refresh_token"] = refreshToken
        }));
    }

    private static OAuthTokenResultDto? ToTokenResult(TruckstopTokenResponse? result)
    {
        if (string.IsNullOrEmpty(result?.AccessToken))
        {
            return null;
        }

        return new OAuthTokenResultDto
        {
            AccessToken = result.AccessToken,
            RefreshToken = result.RefreshToken,
            ExpiresAt = DateTime.UtcNow.AddSeconds(result.ExpiresIn)
        };
    }

    public async Task<IEnumerable<LoadBoardListingDto>> SearchLoadsAsync(LoadBoardSearchCriteria criteria)
    {
        logger.LogInformation("Searching Truckstop loads: Origin={Origin}, Dest={Dest}",
            criteria.OriginAddress?.City, criteria.DestinationAddress?.City);

        var searchRequest = new
        {
            origin = new
            {
                city = criteria.OriginAddress?.City,
                stateProvince = criteria.OriginAddress?.State,
                deadheadMiles = criteria.OriginRadius
            },
            destination = criteria.DestinationAddress != null
                ? new
                {
                    city = criteria.DestinationAddress.City,
                    stateProvince = criteria.DestinationAddress.State,
                    deadheadMiles = criteria.DestinationRadius
                }
                : null,
            pickupDate = criteria.PickupDateStart?.ToString("yyyy-MM-dd"),
            equipmentTypes = criteria.EquipmentTypes,
            pageSize = criteria.MaxResults
        };

        var result = await httpClient.TryPostAsJsonAsync<object, TruckstopSearchResponse>(
            "/loadmanagement-v2/load/search", searchRequest, logger, "Truckstop search loads");

        return result.Value?.Loads?.Select(TruckstopMapper.ToListingDto) ?? [];
    }

    public async Task<LoadBoardListingDto?> GetLoadDetailsAsync(string externalListingId)
    {
        var load = await httpClient.TryGetFromJsonAsync<TruckstopLoad>(
            $"/loadmanagement-v2/load/{externalListingId}", logger, $"Truckstop get load {externalListingId}");

        return load != null ? TruckstopMapper.ToListingDto(load) : null;
    }

    public async Task<LoadBoardBookingResultDto> BookLoadAsync(string externalListingId,
        LoadBoardBookingRequest request)
    {
        logger.LogInformation("Booking Truckstop load {ListingId} for truck {TruckId}",
            externalListingId, request.TruckId);

        var bookRequest = new { loadId = externalListingId, notes = request.Notes };

        var result = await httpClient.TryPostAsJsonAsync<object, TruckstopBookingResponse>(
            $"/loadmanagement-v2/load/{externalListingId}/contact", bookRequest, logger,
            $"Truckstop book load {externalListingId}");

        return result.IsSuccess
            ? new LoadBoardBookingResultDto { Success = true, ExternalConfirmationId = result.Value?.ConfirmationNumber }
            : new LoadBoardBookingResultDto { Success = false, ErrorMessage = $"Truckstop booking failed: {result.ErrorBody}" };
    }

    public async Task<bool> CancelBookingAsync(string externalListingId, string? reason)
    {
        return await httpClient.TryPostAsync(
            $"/loadmanagement-v2/load/{externalListingId}/cancel", new { reason }, logger,
            $"Truckstop cancel booking {externalListingId}");
    }

    public async Task<PostTruckResultDto> PostTruckAsync(PostTruckRequest request)
    {
        logger.LogInformation("Posting truck {TruckId} to Truckstop", request.TruckId);

        var postRequest = new
        {
            origin = new
            {
                city = request.AvailableAtAddress.City,
                stateProvince = request.AvailableAtAddress.State,
                postalCode = request.AvailableAtAddress.ZipCode,
                latitude = request.AvailableAtLocation.Latitude,
                longitude = request.AvailableAtLocation.Longitude
            },
            destination = request.DestinationPreference != null
                ? new
                {
                    city = request.DestinationPreference.City,
                    stateProvince = request.DestinationPreference.State,
                    deadheadMiles = request.DestinationRadius
                }
                : null,
            availableDate = request.AvailableFrom.ToString("yyyy-MM-dd"),
            availableDateEnd = request.AvailableTo?.ToString("yyyy-MM-dd"),
            equipmentType = request.EquipmentType,
            weight = request.MaxWeight,
            length = request.MaxLength
        };

        var result = await httpClient.TryPostAsJsonAsync<object, TruckstopPostTruckResponse>(
            "/truckposting-v2/truck", postRequest, logger, $"Truckstop post truck {request.TruckId}");

        return result.IsSuccess
            ? new PostTruckResultDto
            {
                Success = true, ExternalPostId = result.Value?.TruckId, ExpiresAt = result.Value?.ExpiresAt
            }
            : new PostTruckResultDto { Success = false, ErrorMessage = $"Truckstop post truck failed: {result.ErrorBody}" };
    }

    public async Task<bool> UpdateTruckPostAsync(string externalPostId, PostTruckRequest request)
    {
        var updateRequest = new
        {
            availableDate = request.AvailableFrom.ToString("yyyy-MM-dd"),
            availableDateEnd = request.AvailableTo?.ToString("yyyy-MM-dd"),
            equipmentType = request.EquipmentType,
            weight = request.MaxWeight,
            length = request.MaxLength
        };

        return await httpClient.TryPutAsync(
            $"/truckposting-v2/truck/{externalPostId}", updateRequest, logger,
            $"Truckstop update truck post {externalPostId}");
    }

    public async Task<bool> RemoveTruckPostAsync(string externalPostId)
    {
        return await httpClient.TryDeleteAsync(
            $"/truckposting-v2/truck/{externalPostId}", logger, $"Truckstop remove truck post {externalPostId}");
    }

    public async Task<IEnumerable<PostedTruckDto>> GetPostedTrucksAsync()
    {
        var result = await httpClient.TryGetFromJsonAsync<TruckstopTrucksResponse>(
            "/truckposting-v2/truck", logger, "Truckstop get posted trucks");

        return result?.Trucks?.Select(TruckstopMapper.ToPostedTruckDto) ?? [];
    }

    public Task<LoadBoardWebhookResultDto> ProcessWebhookAsync(string payload, string? signature,
        string? webhookSecret)
    {
        if (!string.IsNullOrEmpty(webhookSecret))
        {
            if (!WebhookSignature.VerifyHmacSha256(payload, signature, webhookSecret))
            {
                logger.LogWarning("Rejected Truckstop webhook with invalid signature");
                return Task.FromResult(new LoadBoardWebhookResultDto
                {
                    IsValid = false,
                    EventType = LoadBoardWebhookEventType.Unknown,
                    ErrorMessage = "Invalid webhook signature"
                });
            }
        }
        else
        {
            logger.LogWarning("Truckstop webhook processed without signature verification - no webhook secret configured");
        }

        try
        {
            var webhook = JsonSerializer.Deserialize<TruckstopWebhookPayload>(payload);
            return Task.FromResult(new LoadBoardWebhookResultDto
            {
                IsValid = true,
                EventType = TruckstopMapper.MapWebhookEventType(webhook?.Event),
                ExternalListingId = webhook?.LoadId
            });
        }
        catch (JsonException ex)
        {
            logger.LogError(ex, "Error processing Truckstop webhook");
            return Task.FromResult(new LoadBoardWebhookResultDto
            {
                IsValid = false, EventType = LoadBoardWebhookEventType.Unknown, ErrorMessage = ex.Message
            });
        }
    }

    private async Task<TruckstopTokenResponse?> RequestTokenAsync(string action, Dictionary<string, string> form)
    {
        try
        {
            var authClient = httpClientFactory.CreateClient();
            var response = await authClient.PostAsync(options.TokenUrl, new FormUrlEncodedContent(form));
            if (!response.IsSuccessStatusCode)
            {
                logger.LogWarning("Truckstop token {Action} failed: {StatusCode}", action, response.StatusCode);
                return null;
            }

            return await response.Content.ReadFromJsonAsync<TruckstopTokenResponse>();
        }
        catch (Exception ex) when (ex is HttpRequestException or JsonException or TaskCanceledException)
        {
            logger.LogError(ex, "Error during Truckstop token {Action}", action);
            return null;
        }
    }
}
