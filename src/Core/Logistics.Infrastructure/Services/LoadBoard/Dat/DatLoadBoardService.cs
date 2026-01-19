using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Logistics.Application.Services;
using Logistics.Domain.Entities;
using Logistics.Domain.Primitives.Enums;
using Logistics.Shared.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Logistics.Infrastructure.Services.Dat;

/// <summary>
///     DAT Load Board provider implementation.
///     Integrates with DAT Freight &amp; Analytics API for searching loads and posting trucks.
///     Authentication: Bearer Token (Service Account or User Account)
///     Base URL: https://freight.api.dat.com
/// </summary>
internal class DatLoadBoardService(
    HttpClient httpClient,
    IOptions<LoadBoardOptions> options,
    ILogger<DatLoadBoardService> logger)
    : ILoadBoardProviderService
{
    private readonly DatOptions _options = options.Value.Dat ?? new DatOptions();
    private string? _accessToken;
    private DateTime _tokenExpiry = DateTime.MinValue;

    public LoadBoardProviderType ProviderType => LoadBoardProviderType.Dat;

    public void Initialize(LoadBoardConfiguration configuration)
    {
        _accessToken = configuration.AccessToken;
        _tokenExpiry = configuration.TokenExpiresAt ?? DateTime.MinValue;

        httpClient.BaseAddress = new Uri(_options.BaseUrl);

        if (!string.IsNullOrEmpty(_accessToken))
        {
            httpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", _accessToken);
        }

        logger.LogInformation("Initialized DAT Load Board provider");
    }

    public async Task<bool> ValidateCredentialsAsync(string apiKey, string? apiSecret)
    {
        try
        {
            var authRequest = new { clientId = apiKey, clientSecret = apiSecret };

            using var authClient = new HttpClient();
            var response = await authClient.PostAsJsonAsync(_options.AuthUrl, authRequest);

            if (response.IsSuccessStatusCode)
            {
                logger.LogInformation("DAT credentials validated successfully");
                return true;
            }

            logger.LogWarning("DAT credential validation failed: {StatusCode}", response.StatusCode);
            return false;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error validating DAT credentials");
            return false;
        }
    }

    public Task<OAuthTokenResultDto?> RefreshTokenAsync(string refreshToken)
    {
        logger.LogDebug("DAT does not support token refresh - use client credentials to obtain new token");
        return Task.FromResult<OAuthTokenResultDto?>(null);
    }

    public async Task<IEnumerable<LoadBoardListingDto>> SearchLoadsAsync(LoadBoardSearchCriteria criteria)
    {
        logger.LogInformation("Searching DAT loads: Origin={Origin}, Dest={Dest}",
            criteria.OriginAddress?.City, criteria.DestinationAddress?.City);

        try
        {
            var searchRequest = new
            {
                origin = new
                    { city = criteria.OriginAddress?.City, state = criteria.OriginAddress?.State, radius = criteria.OriginRadius },
                destination = criteria.DestinationAddress != null
                    ? new
                    {
                        city = criteria.DestinationAddress.City, state = criteria.DestinationAddress.State,
                        radius = criteria.DestinationRadius
                    }
                    : null,
                pickupDate = new
                {
                    start = criteria.PickupDateStart?.ToString("yyyy-MM-dd"),
                    end = criteria.PickupDateEnd?.ToString("yyyy-MM-dd")
                },
                equipmentTypes = criteria.EquipmentTypes,
                limit = criteria.MaxResults
            };

            var response = await httpClient.PostAsJsonAsync("/freight/v3/loads/search", searchRequest);

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                logger.LogWarning("DAT search failed: {StatusCode} - {Error}", response.StatusCode, error);
                return [];
            }

            var result = await response.Content.ReadFromJsonAsync<DatSearchResponse>();
            return result?.Loads?.Select(DatMapper.ToListingDto) ?? [];
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error searching DAT loads");
            return [];
        }
    }

    public async Task<LoadBoardListingDto?> GetLoadDetailsAsync(string externalListingId)
    {
        try
        {
            var response = await httpClient.GetAsync($"/freight/v3/loads/{externalListingId}");

            if (!response.IsSuccessStatusCode)
            {
                logger.LogWarning("DAT get load details failed: {StatusCode}", response.StatusCode);
                return null;
            }

            var load = await response.Content.ReadFromJsonAsync<DatLoad>();
            return load != null ? DatMapper.ToListingDto(load) : null;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error getting DAT load details for {ListingId}", externalListingId);
            return null;
        }
    }

    public async Task<LoadBoardBookingResultDto> BookLoadAsync(string externalListingId,
        LoadBoardBookingRequest request)
    {
        logger.LogInformation("Booking DAT load {ListingId} for truck {TruckId}", externalListingId, request.TruckId);

        try
        {
            var bookRequest = new
                { loadId = externalListingId, truckId = request.TruckId.ToString(), notes = request.Notes };
            var response = await httpClient.PostAsJsonAsync($"/freight/v3/loads/{externalListingId}/book", bookRequest);

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                logger.LogWarning("DAT booking failed: {StatusCode} - {Error}", response.StatusCode, error);
                return new LoadBoardBookingResultDto { Success = false, ErrorMessage = $"DAT booking failed: {error}" };
            }

            var result = await response.Content.ReadFromJsonAsync<DatBookingResponse>();
            return new LoadBoardBookingResultDto { Success = true, ExternalConfirmationId = result?.ConfirmationId };
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error booking DAT load {ListingId}", externalListingId);
            return new LoadBoardBookingResultDto { Success = false, ErrorMessage = ex.Message };
        }
    }

    public async Task<bool> CancelBookingAsync(string externalListingId, string? reason)
    {
        try
        {
            var response =
                await httpClient.PostAsJsonAsync($"/freight/v3/loads/{externalListingId}/cancel", new { reason });
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error cancelling DAT booking {ListingId}", externalListingId);
            return false;
        }
    }

    public async Task<PostTruckResultDto> PostTruckAsync(PostTruckRequest request)
    {
        logger.LogInformation("Posting truck {TruckId} to DAT", request.TruckId);

        try
        {
            var postRequest = new
            {
                origin = new
                {
                    city = request.AvailableAtAddress.City,
                    state = request.AvailableAtAddress.State,
                    latitude = request.AvailableAtLocation.Latitude,
                    longitude = request.AvailableAtLocation.Longitude
                },
                destination = request.DestinationPreference != null
                    ? new
                    {
                        city = request.DestinationPreference.City, state = request.DestinationPreference.State,
                        radius = request.DestinationRadius
                    }
                    : null,
                availableDate = new
                {
                    start = request.AvailableFrom.ToString("yyyy-MM-dd"),
                    end = request.AvailableTo?.ToString("yyyy-MM-dd")
                },
                equipmentType = request.EquipmentType,
                maxWeight = request.MaxWeight,
                maxLength = request.MaxLength
            };

            var response = await httpClient.PostAsJsonAsync("/freight/v3/trucks", postRequest);

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                logger.LogWarning("DAT post truck failed: {StatusCode} - {Error}", response.StatusCode, error);
                return new PostTruckResultDto { Success = false, ErrorMessage = $"DAT post truck failed: {error}" };
            }

            var result = await response.Content.ReadFromJsonAsync<DatPostTruckResponse>();
            return new PostTruckResultDto
                { Success = true, ExternalPostId = result?.PostId, ExpiresAt = result?.ExpiresAt };
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error posting truck {TruckId} to DAT", request.TruckId);
            return new PostTruckResultDto { Success = false, ErrorMessage = ex.Message };
        }
    }

    public async Task<bool> UpdateTruckPostAsync(string externalPostId, PostTruckRequest request)
    {
        try
        {
            var updateRequest = new
            {
                availableDate = new
                {
                    start = request.AvailableFrom.ToString("yyyy-MM-dd"),
                    end = request.AvailableTo?.ToString("yyyy-MM-dd")
                },
                equipmentType = request.EquipmentType,
                maxWeight = request.MaxWeight,
                maxLength = request.MaxLength
            };

            var response = await httpClient.PutAsJsonAsync($"/freight/v3/trucks/{externalPostId}", updateRequest);
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error updating DAT truck post {PostId}", externalPostId);
            return false;
        }
    }

    public async Task<bool> RemoveTruckPostAsync(string externalPostId)
    {
        try
        {
            var response = await httpClient.DeleteAsync($"/freight/v3/trucks/{externalPostId}");
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error removing DAT truck post {PostId}", externalPostId);
            return false;
        }
    }

    public async Task<IEnumerable<PostedTruckDto>> GetPostedTrucksAsync()
    {
        try
        {
            var response = await httpClient.GetAsync("/freight/v3/trucks");
            if (!response.IsSuccessStatusCode)
            {
                return [];
            }

            var result = await response.Content.ReadFromJsonAsync<DatTrucksResponse>();
            return result?.Trucks?.Select(DatMapper.ToPostedTruckDto) ?? [];
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error getting DAT posted trucks");
            return [];
        }
    }

    public async Task<LoadBoardWebhookResultDto> ProcessWebhookAsync(string payload, string? signature,
        string? webhookSecret)
    {
        try
        {
            var webhook = JsonSerializer.Deserialize<DatWebhookPayload>(payload);
            return new LoadBoardWebhookResultDto
            {
                IsValid = true,
                EventType = DatMapper.MapWebhookEventType(webhook?.EventType),
                ExternalListingId = webhook?.LoadId
            };
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error processing DAT webhook");
            return new LoadBoardWebhookResultDto
                { IsValid = false, EventType = LoadBoardWebhookEventType.Unknown, ErrorMessage = ex.Message };
        }
    }
}
