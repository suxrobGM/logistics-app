using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Logistics.Application.Services;
using Logistics.Domain.Entities;
using Logistics.Domain.Primitives.Enums;
using Logistics.Shared.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Logistics.Infrastructure.Integrations.LoadBoard.Providers.Truckstop;

/// <summary>
///     Truckstop.com Load Board provider implementation.
///     Authentication: OAuth 2.0 (Resource Owner Password grant)
///     Access Token validity: 20 minutes, Refresh Token validity: 6 months
/// </summary>
internal class TruckstopLoadBoardService(
    HttpClient httpClient,
    IOptions<LoadBoardOptions> options,
    ILogger<TruckstopLoadBoardService> logger)
    : ILoadBoardProviderService
{
    private readonly TruckstopOptions _options = options.Value.Truckstop ?? new TruckstopOptions();
    private string? _accessToken;
    private string? _refreshToken;
    private DateTime _tokenExpiry = DateTime.MinValue;

    public LoadBoardProviderType ProviderType => LoadBoardProviderType.Truckstop;

    public void Initialize(LoadBoardConfiguration configuration)
    {
        _accessToken = configuration.AccessToken;
        _refreshToken = configuration.RefreshToken;
        _tokenExpiry = configuration.TokenExpiresAt ?? DateTime.MinValue;

        httpClient.BaseAddress = new Uri(_options.BaseUrl);

        if (!string.IsNullOrEmpty(_accessToken))
        {
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _accessToken);
        }

        logger.LogInformation("Initialized Truckstop Load Board provider");
    }

    public async Task<bool> ValidateCredentialsAsync(string apiKey, string? apiSecret)
    {
        try
        {
            var tokenResult = await GetTokenAsync(apiKey, apiSecret);
            return tokenResult != null;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error validating Truckstop credentials");
            return false;
        }
    }

    public async Task<OAuthTokenResultDto?> RefreshTokenAsync(string refreshToken)
    {
        try
        {
            using var authClient = new HttpClient();
            var tokenRequest = new Dictionary<string, string>
            {
                ["grant_type"] = "refresh_token", ["refresh_token"] = refreshToken
            };

            var response = await authClient.PostAsync(_options.TokenUrl, new FormUrlEncodedContent(tokenRequest));

            if (!response.IsSuccessStatusCode)
            {
                logger.LogWarning("Truckstop token refresh failed: {StatusCode}", response.StatusCode);
                return null;
            }

            var result = await response.Content.ReadFromJsonAsync<TruckstopTokenResponse>();
            if (result == null)
            {
                return null;
            }

            _accessToken = result.AccessToken;
            _refreshToken = result.RefreshToken;
            _tokenExpiry = DateTime.UtcNow.AddSeconds(result.ExpiresIn);

            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _accessToken);

            return new OAuthTokenResultDto
            {
                AccessToken = result.AccessToken, RefreshToken = result.RefreshToken, ExpiresAt = _tokenExpiry
            };
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error refreshing Truckstop token");
            return null;
        }
    }

    public async Task<IEnumerable<LoadBoardListingDto>> SearchLoadsAsync(LoadBoardSearchCriteria criteria)
    {
        logger.LogInformation("Searching Truckstop loads: Origin={Origin}, Dest={Dest}", criteria.OriginAddress?.City,
            criteria.DestinationAddress?.City);

        try
        {
            await EnsureValidTokenAsync();

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

            var response = await httpClient.PostAsJsonAsync("/loadmanagement-v2/load/search", searchRequest);

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                logger.LogWarning("Truckstop search failed: {StatusCode} - {Error}", response.StatusCode, error);
                return [];
            }

            var result = await response.Content.ReadFromJsonAsync<TruckstopSearchResponse>();
            return result?.Loads?.Select(TruckstopMapper.ToListingDto) ?? [];
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error searching Truckstop loads");
            return [];
        }
    }

    public async Task<LoadBoardListingDto?> GetLoadDetailsAsync(string externalListingId)
    {
        try
        {
            await EnsureValidTokenAsync();
            var response = await httpClient.GetAsync($"/loadmanagement-v2/load/{externalListingId}");

            if (!response.IsSuccessStatusCode)
            {
                logger.LogWarning("Truckstop get load details failed: {StatusCode}", response.StatusCode);
                return null;
            }

            var load = await response.Content.ReadFromJsonAsync<TruckstopLoad>();
            return load != null ? TruckstopMapper.ToListingDto(load) : null;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error getting Truckstop load details for {ListingId}", externalListingId);
            return null;
        }
    }

    public async Task<LoadBoardBookingResultDto> BookLoadAsync(string externalListingId,
        LoadBoardBookingRequest request)
    {
        logger.LogInformation("Booking Truckstop load {ListingId} for truck {TruckId}", externalListingId,
            request.TruckId);

        try
        {
            await EnsureValidTokenAsync();
            var bookRequest = new { loadId = externalListingId, notes = request.Notes };
            var response =
                await httpClient.PostAsJsonAsync($"/loadmanagement-v2/load/{externalListingId}/contact", bookRequest);

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                logger.LogWarning("Truckstop booking failed: {StatusCode} - {Error}", response.StatusCode, error);
                return new LoadBoardBookingResultDto
                {
                    Success = false, ErrorMessage = $"Truckstop booking failed: {error}"
                };
            }

            var result = await response.Content.ReadFromJsonAsync<TruckstopBookingResponse>();
            return new LoadBoardBookingResultDto
            {
                Success = true, ExternalConfirmationId = result?.ConfirmationNumber
            };
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error booking Truckstop load {ListingId}", externalListingId);
            return new LoadBoardBookingResultDto { Success = false, ErrorMessage = ex.Message };
        }
    }

    public async Task<bool> CancelBookingAsync(string externalListingId, string? reason)
    {
        try
        {
            await EnsureValidTokenAsync();
            var response = await httpClient.PostAsJsonAsync($"/loadmanagement-v2/load/{externalListingId}/cancel",
                new { reason });
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error cancelling Truckstop booking {ListingId}", externalListingId);
            return false;
        }
    }

    public async Task<PostTruckResultDto> PostTruckAsync(PostTruckRequest request)
    {
        logger.LogInformation("Posting truck {TruckId} to Truckstop", request.TruckId);

        try
        {
            await EnsureValidTokenAsync();

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

            var response = await httpClient.PostAsJsonAsync("/truckposting-v2/truck", postRequest);

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                logger.LogWarning("Truckstop post truck failed: {StatusCode} - {Error}", response.StatusCode, error);
                return new PostTruckResultDto
                {
                    Success = false, ErrorMessage = $"Truckstop post truck failed: {error}"
                };
            }

            var result = await response.Content.ReadFromJsonAsync<TruckstopPostTruckResponse>();
            return new PostTruckResultDto
            {
                Success = true, ExternalPostId = result?.TruckId, ExpiresAt = result?.ExpiresAt
            };
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error posting truck {TruckId} to Truckstop", request.TruckId);
            return new PostTruckResultDto { Success = false, ErrorMessage = ex.Message };
        }
    }

    public async Task<bool> UpdateTruckPostAsync(string externalPostId, PostTruckRequest request)
    {
        try
        {
            await EnsureValidTokenAsync();
            var updateRequest = new
            {
                availableDate = request.AvailableFrom.ToString("yyyy-MM-dd"),
                availableDateEnd = request.AvailableTo?.ToString("yyyy-MM-dd"),
                equipmentType = request.EquipmentType,
                weight = request.MaxWeight,
                length = request.MaxLength
            };

            var response = await httpClient.PutAsJsonAsync($"/truckposting-v2/truck/{externalPostId}", updateRequest);
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error updating Truckstop truck post {PostId}", externalPostId);
            return false;
        }
    }

    public async Task<bool> RemoveTruckPostAsync(string externalPostId)
    {
        try
        {
            await EnsureValidTokenAsync();
            var response = await httpClient.DeleteAsync($"/truckposting-v2/truck/{externalPostId}");
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error removing Truckstop truck post {PostId}", externalPostId);
            return false;
        }
    }

    public async Task<IEnumerable<PostedTruckDto>> GetPostedTrucksAsync()
    {
        try
        {
            await EnsureValidTokenAsync();
            var response = await httpClient.GetAsync("/truckposting-v2/truck");

            if (!response.IsSuccessStatusCode)
            {
                return [];
            }

            var result = await response.Content.ReadFromJsonAsync<TruckstopTrucksResponse>();
            return result?.Trucks?.Select(TruckstopMapper.ToPostedTruckDto) ?? [];
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error getting Truckstop posted trucks");
            return [];
        }
    }

    public async Task<LoadBoardWebhookResultDto> ProcessWebhookAsync(string payload, string? signature,
        string? webhookSecret)
    {
        try
        {
            var webhook = JsonSerializer.Deserialize<TruckstopWebhookPayload>(payload);
            return new LoadBoardWebhookResultDto
            {
                IsValid = true,
                EventType = TruckstopMapper.MapWebhookEventType(webhook?.Event),
                ExternalListingId = webhook?.LoadId
            };
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error processing Truckstop webhook");
            return new LoadBoardWebhookResultDto
            {
                IsValid = false, EventType = LoadBoardWebhookEventType.Unknown, ErrorMessage = ex.Message
            };
        }
    }

    private async Task<TruckstopTokenResponse?> GetTokenAsync(string username, string? password)
    {
        using var authClient = new HttpClient();
        var tokenRequest = new Dictionary<string, string>
        {
            ["grant_type"] = "password", ["username"] = username, ["password"] = password ?? string.Empty
        };

        var response = await authClient.PostAsync(_options.TokenUrl, new FormUrlEncodedContent(tokenRequest));
        if (!response.IsSuccessStatusCode)
        {
            return null;
        }

        return await response.Content.ReadFromJsonAsync<TruckstopTokenResponse>();
    }

    private async Task EnsureValidTokenAsync()
    {
        if (DateTime.UtcNow < _tokenExpiry.AddMinutes(-2))
        {
            return;
        }

        if (!string.IsNullOrEmpty(_refreshToken))
        {
            var result = await RefreshTokenAsync(_refreshToken);
            if (result != null)
            {
                return;
            }
        }

        logger.LogWarning("Truckstop token expired and refresh failed");
    }
}
