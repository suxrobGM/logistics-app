using System.Net.Http.Json;
using System.Text.Json;
using Logistics.Application.Services;
using Logistics.Domain.Entities;
using Logistics.Domain.Primitives.Enums;
using Logistics.Shared.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Logistics.Infrastructure.Services.OneTwo3;

/// <summary>
///     123Loadboard provider implementation.
///     Authentication: API Key (X-API-Key header)
///     Rate Limits: 100 searches/hour, 300/day, 2000/month
/// </summary>
internal class OneTwo3LoadBoardService(
    HttpClient httpClient,
    IOptions<LoadBoardOptions> options,
    ILogger<OneTwo3LoadBoardService> logger)
    : ILoadBoardProviderService
{
    private readonly OneTwo3LoadboardOptions _options = options.Value.OneTwo3Loadboard ?? new OneTwo3LoadboardOptions();
    private readonly Lock _rateLimitLock = new();
    private string? _apiKey;
    private int _dailySearchCount;

    private int _hourlySearchCount;
    private DateTime _lastDayReset = DateTime.UtcNow;
    private DateTime _lastHourReset = DateTime.UtcNow;

    public LoadBoardProviderType ProviderType => LoadBoardProviderType.OneTwo3Loadboard;

    public void Initialize(LoadBoardConfiguration configuration)
    {
        _apiKey = configuration.ApiKey;
        httpClient.BaseAddress = new Uri(_options.BaseUrl);

        if (!string.IsNullOrEmpty(_apiKey))
        {
            httpClient.DefaultRequestHeaders.Add("X-API-Key", _apiKey);
        }

        logger.LogInformation("Initialized 123Loadboard provider");
    }

    public async Task<bool> ValidateCredentialsAsync(string apiKey, string? apiSecret)
    {
        try
        {
            using var testClient = new HttpClient();
            testClient.BaseAddress = new Uri(_options.BaseUrl);
            testClient.DefaultRequestHeaders.Add("X-API-Key", apiKey);

            var response = await testClient.GetAsync("/v1/account");

            if (response.IsSuccessStatusCode)
            {
                logger.LogInformation("123Loadboard credentials validated successfully");
                return true;
            }

            logger.LogWarning("123Loadboard credential validation failed: {StatusCode}", response.StatusCode);
            return false;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error validating 123Loadboard credentials");
            return false;
        }
    }

    public Task<OAuthTokenResultDto?> RefreshTokenAsync(string refreshToken)
    {
        logger.LogDebug("123Loadboard uses API key authentication - token refresh not applicable");
        return Task.FromResult<OAuthTokenResultDto?>(null);
    }

    public async Task<IEnumerable<LoadBoardListingDto>> SearchLoadsAsync(LoadBoardSearchCriteria criteria)
    {
        if (!CheckRateLimit())
        {
            logger.LogWarning("123Loadboard rate limit exceeded");
            return [];
        }

        logger.LogInformation("Searching 123Loadboard loads: Origin={Origin}, Dest={Dest}", criteria.OriginAddress?.City,
            criteria.DestinationAddress?.City);

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
                pickupDateStart = criteria.PickupDateStart?.ToString("yyyy-MM-dd"),
                pickupDateEnd = criteria.PickupDateEnd?.ToString("yyyy-MM-dd"),
                equipmentTypes = criteria.EquipmentTypes,
                limit = criteria.MaxResults
            };

            var response = await httpClient.PostAsJsonAsync("/v1/loads/search", searchRequest);
            IncrementSearchCount();

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                logger.LogWarning("123Loadboard search failed: {StatusCode} - {Error}", response.StatusCode, error);
                return [];
            }

            var result = await response.Content.ReadFromJsonAsync<OneTwo3SearchResponse>();
            return result?.Loads?.Select(OneTwo3Mapper.ToListingDto) ?? [];
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error searching 123Loadboard loads");
            return [];
        }
    }

    public async Task<LoadBoardListingDto?> GetLoadDetailsAsync(string externalListingId)
    {
        try
        {
            var response = await httpClient.GetAsync($"/v1/loads/{externalListingId}");

            if (!response.IsSuccessStatusCode)
            {
                logger.LogWarning("123Loadboard get load details failed: {StatusCode}", response.StatusCode);
                return null;
            }

            var load = await response.Content.ReadFromJsonAsync<OneTwo3Load>();
            return load != null ? OneTwo3Mapper.ToListingDto(load) : null;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error getting 123Loadboard load details for {ListingId}", externalListingId);
            return null;
        }
    }

    public async Task<LoadBoardBookingResultDto> BookLoadAsync(string externalListingId,
        LoadBoardBookingRequest request)
    {
        logger.LogInformation("Booking 123Loadboard load {ListingId} for truck {TruckId}", externalListingId,
            request.TruckId);

        try
        {
            var bookRequest = new { loadId = externalListingId, message = request.Notes };
            var response = await httpClient.PostAsJsonAsync($"/v1/loads/{externalListingId}/contact", bookRequest);

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                logger.LogWarning("123Loadboard booking failed: {StatusCode} - {Error}", response.StatusCode, error);
                return new LoadBoardBookingResultDto
                    { Success = false, ErrorMessage = $"123Loadboard booking failed: {error}" };
            }

            var result = await response.Content.ReadFromJsonAsync<OneTwo3BookingResponse>();
            return new LoadBoardBookingResultDto { Success = true, ExternalConfirmationId = result?.ReferenceId };
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error booking 123Loadboard load {ListingId}", externalListingId);
            return new LoadBoardBookingResultDto { Success = false, ErrorMessage = ex.Message };
        }
    }

    public async Task<bool> CancelBookingAsync(string externalListingId, string? reason)
    {
        try
        {
            var response = await httpClient.PostAsJsonAsync($"/v1/loads/{externalListingId}/cancel", new { reason });
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error cancelling 123Loadboard booking {ListingId}", externalListingId);
            return false;
        }
    }

    public async Task<PostTruckResultDto> PostTruckAsync(PostTruckRequest request)
    {
        logger.LogInformation("Posting truck {TruckId} to 123Loadboard", request.TruckId);

        try
        {
            var postRequest = new
            {
                origin = new
                {
                    city = request.AvailableAtAddress.City,
                    state = request.AvailableAtAddress.State,
                    zipCode = request.AvailableAtAddress.ZipCode,
                    lat = request.AvailableAtLocation.Latitude,
                    lng = request.AvailableAtLocation.Longitude
                },
                destination = request.DestinationPreference != null
                    ? new
                    {
                        city = request.DestinationPreference.City, state = request.DestinationPreference.State,
                        radius = request.DestinationRadius
                    }
                    : null,
                availableFrom = request.AvailableFrom.ToString("yyyy-MM-ddTHH:mm:ssZ"),
                availableTo = request.AvailableTo?.ToString("yyyy-MM-ddTHH:mm:ssZ"),
                equipment = request.EquipmentType,
                maxWeight = request.MaxWeight,
                maxLength = request.MaxLength
            };

            var response = await httpClient.PostAsJsonAsync("/v1/trucks", postRequest);

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                logger.LogWarning("123Loadboard post truck failed: {StatusCode} - {Error}", response.StatusCode, error);
                return new PostTruckResultDto
                    { Success = false, ErrorMessage = $"123Loadboard post truck failed: {error}" };
            }

            var result = await response.Content.ReadFromJsonAsync<OneTwo3PostTruckResponse>();
            return new PostTruckResultDto
                { Success = true, ExternalPostId = result?.TruckPostId, ExpiresAt = result?.ExpiresAt };
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error posting truck {TruckId} to 123Loadboard", request.TruckId);
            return new PostTruckResultDto { Success = false, ErrorMessage = ex.Message };
        }
    }

    public async Task<bool> UpdateTruckPostAsync(string externalPostId, PostTruckRequest request)
    {
        try
        {
            var updateRequest = new
            {
                availableFrom = request.AvailableFrom.ToString("yyyy-MM-ddTHH:mm:ssZ"),
                availableTo = request.AvailableTo?.ToString("yyyy-MM-ddTHH:mm:ssZ"),
                equipment = request.EquipmentType,
                maxWeight = request.MaxWeight,
                maxLength = request.MaxLength
            };

            var response = await httpClient.PutAsJsonAsync($"/v1/trucks/{externalPostId}", updateRequest);
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error updating 123Loadboard truck post {PostId}", externalPostId);
            return false;
        }
    }

    public async Task<bool> RemoveTruckPostAsync(string externalPostId)
    {
        try
        {
            var response = await httpClient.DeleteAsync($"/v1/trucks/{externalPostId}");
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error removing 123Loadboard truck post {PostId}", externalPostId);
            return false;
        }
    }

    public async Task<IEnumerable<PostedTruckDto>> GetPostedTrucksAsync()
    {
        try
        {
            var response = await httpClient.GetAsync("/v1/trucks");
            if (!response.IsSuccessStatusCode)
            {
                return [];
            }

            var result = await response.Content.ReadFromJsonAsync<OneTwo3TrucksResponse>();
            return result?.Trucks?.Select(OneTwo3Mapper.ToPostedTruckDto) ?? [];
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error getting 123Loadboard posted trucks");
            return [];
        }
    }

    public async Task<LoadBoardWebhookResultDto> ProcessWebhookAsync(string payload, string? signature,
        string? webhookSecret)
    {
        try
        {
            var webhook = JsonSerializer.Deserialize<OneTwo3WebhookPayload>(payload);
            return new LoadBoardWebhookResultDto
            {
                IsValid = true,
                EventType = OneTwo3Mapper.MapWebhookEventType(webhook?.EventType),
                ExternalListingId = webhook?.LoadId
            };
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error processing 123Loadboard webhook");
            return new LoadBoardWebhookResultDto
                { IsValid = false, EventType = LoadBoardWebhookEventType.Unknown, ErrorMessage = ex.Message };
        }
    }

    private bool CheckRateLimit()
    {
        lock (_rateLimitLock)
        {
            var now = DateTime.UtcNow;

            if ((now - _lastHourReset).TotalHours >= 1)
            {
                _hourlySearchCount = 0;
                _lastHourReset = now;
            }

            if ((now - _lastDayReset).TotalDays >= 1)
            {
                _dailySearchCount = 0;
                _lastDayReset = now;
            }

            if (_hourlySearchCount >= _options.MaxSearchesPerHour)
            {
                logger.LogWarning("123Loadboard hourly rate limit reached: {Count}/{Max}", _hourlySearchCount,
                    _options.MaxSearchesPerHour);
                return false;
            }

            if (_dailySearchCount >= _options.MaxSearchesPerDay)
            {
                logger.LogWarning("123Loadboard daily rate limit reached: {Count}/{Max}", _dailySearchCount,
                    _options.MaxSearchesPerDay);
                return false;
            }

            return true;
        }
    }

    private void IncrementSearchCount()
    {
        lock (_rateLimitLock)
        {
            _hourlySearchCount++;
            _dailySearchCount++;
        }
    }
}
