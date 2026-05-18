using System.Text.Json;
using Logistics.Domain.Entities;
using Logistics.Domain.Primitives.Enums;
using Logistics.Infrastructure.Integrations.LoadBoard.Common;
using Logistics.Shared.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Logistics.Application.Abstractions.LoadBoard;

namespace Logistics.Infrastructure.Integrations.LoadBoard.Providers.OneTwo3;

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
    private readonly OneTwo3LoadboardOptions options = options.Value.OneTwo3Loadboard ?? new OneTwo3LoadboardOptions();
    private readonly Lock rateLimitLock = new();
    private int dailySearchCount;
    private int hourlySearchCount;
    private DateTime lastDayReset = DateTime.UtcNow;
    private DateTime lastHourReset = DateTime.UtcNow;
    private string? serviceApiKey;

    public LoadBoardProviderType ProviderType => LoadBoardProviderType.OneTwo3Loadboard;

    public void Initialize(LoadBoardConfiguration configuration)
    {
        serviceApiKey = configuration.ApiKey;
        httpClient.BaseAddress = new Uri(options.BaseUrl);

        if (!string.IsNullOrEmpty(serviceApiKey))
        {
            httpClient.DefaultRequestHeaders.Add("X-API-Key", serviceApiKey);
        }

        logger.LogInformation("Initialized 123Loadboard provider");
    }

    public async Task<bool> ValidateCredentialsAsync(string apiKey, string? apiSecret)
    {
        try
        {
            using var testClient = new HttpClient();
            testClient.BaseAddress = new Uri(options.BaseUrl);
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
        catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException)
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

        logger.LogInformation("Searching 123Loadboard loads: Origin={Origin}, Dest={Dest}",
            criteria.OriginAddress?.City, criteria.DestinationAddress?.City);

        var searchRequest = new
        {
            origin = new
            {
                city = criteria.OriginAddress?.City,
                state = criteria.OriginAddress?.State,
                radius = criteria.OriginRadius
            },
            destination = criteria.DestinationAddress != null
                ? new
                {
                    city = criteria.DestinationAddress.City,
                    state = criteria.DestinationAddress.State,
                    radius = criteria.DestinationRadius
                }
                : null,
            pickupDateStart = criteria.PickupDateStart?.ToString("yyyy-MM-dd"),
            pickupDateEnd = criteria.PickupDateEnd?.ToString("yyyy-MM-dd"),
            equipmentTypes = criteria.EquipmentTypes,
            limit = criteria.MaxResults
        };

        var result = await httpClient.TryPostAsJsonAsync<object, OneTwo3SearchResponse>(
            "/v1/loads/search", searchRequest, logger, "123Loadboard search loads");

        IncrementSearchCount();
        return result.Value?.Loads?.Select(OneTwo3Mapper.ToListingDto) ?? [];
    }

    public async Task<LoadBoardListingDto?> GetLoadDetailsAsync(string externalListingId)
    {
        var load = await httpClient.TryGetFromJsonAsync<OneTwo3Load>(
            $"/v1/loads/{externalListingId}", logger, $"123Loadboard get load {externalListingId}");

        return load != null ? OneTwo3Mapper.ToListingDto(load) : null;
    }

    public async Task<LoadBoardBookingResultDto> BookLoadAsync(string externalListingId,
        LoadBoardBookingRequest request)
    {
        logger.LogInformation("Booking 123Loadboard load {ListingId} for truck {TruckId}",
            externalListingId, request.TruckId);

        var bookRequest = new { loadId = externalListingId, message = request.Notes };

        var result = await httpClient.TryPostAsJsonAsync<object, OneTwo3BookingResponse>(
            $"/v1/loads/{externalListingId}/contact", bookRequest, logger,
            $"123Loadboard book load {externalListingId}");

        return result.IsSuccess
            ? new LoadBoardBookingResultDto { Success = true, ExternalConfirmationId = result.Value?.ReferenceId }
            : new LoadBoardBookingResultDto { Success = false, ErrorMessage = $"123Loadboard booking failed: {result.ErrorBody}" };
    }

    public Task<bool> CancelBookingAsync(string externalListingId, string? reason)
    {
        return httpClient.TryPostAsync(
            $"/v1/loads/{externalListingId}/cancel", new { reason }, logger,
            $"123Loadboard cancel booking {externalListingId}");
    }

    public async Task<PostTruckResultDto> PostTruckAsync(PostTruckRequest request)
    {
        logger.LogInformation("Posting truck {TruckId} to 123Loadboard", request.TruckId);

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
                    city = request.DestinationPreference.City,
                    state = request.DestinationPreference.State,
                    radius = request.DestinationRadius
                }
                : null,
            availableFrom = request.AvailableFrom.ToString("yyyy-MM-ddTHH:mm:ssZ"),
            availableTo = request.AvailableTo?.ToString("yyyy-MM-ddTHH:mm:ssZ"),
            equipment = request.EquipmentType,
            maxWeight = request.MaxWeight,
            maxLength = request.MaxLength
        };

        var result = await httpClient.TryPostAsJsonAsync<object, OneTwo3PostTruckResponse>(
            "/v1/trucks", postRequest, logger, $"123Loadboard post truck {request.TruckId}");

        return result.IsSuccess
            ? new PostTruckResultDto
            {
                Success = true, ExternalPostId = result.Value?.TruckPostId, ExpiresAt = result.Value?.ExpiresAt
            }
            : new PostTruckResultDto { Success = false, ErrorMessage = $"123Loadboard post truck failed: {result.ErrorBody}" };
    }

    public Task<bool> UpdateTruckPostAsync(string externalPostId, PostTruckRequest request)
    {
        var updateRequest = new
        {
            availableFrom = request.AvailableFrom.ToString("yyyy-MM-ddTHH:mm:ssZ"),
            availableTo = request.AvailableTo?.ToString("yyyy-MM-ddTHH:mm:ssZ"),
            equipment = request.EquipmentType,
            maxWeight = request.MaxWeight,
            maxLength = request.MaxLength
        };

        return httpClient.TryPutAsync(
            $"/v1/trucks/{externalPostId}", updateRequest, logger, $"123Loadboard update truck post {externalPostId}");
    }

    public Task<bool> RemoveTruckPostAsync(string externalPostId)
    {
        return httpClient.TryDeleteAsync(
            $"/v1/trucks/{externalPostId}", logger, $"123Loadboard remove truck post {externalPostId}");
    }

    public async Task<IEnumerable<PostedTruckDto>> GetPostedTrucksAsync()
    {
        var result = await httpClient.TryGetFromJsonAsync<OneTwo3TrucksResponse>(
            "/v1/trucks", logger, "123Loadboard get posted trucks");

        return result?.Trucks?.Select(OneTwo3Mapper.ToPostedTruckDto) ?? [];
    }

    public Task<LoadBoardWebhookResultDto> ProcessWebhookAsync(string payload, string? signature,
        string? webhookSecret)
    {
        try
        {
            var webhook = JsonSerializer.Deserialize<OneTwo3WebhookPayload>(payload);
            return Task.FromResult(new LoadBoardWebhookResultDto
            {
                IsValid = true,
                EventType = OneTwo3Mapper.MapWebhookEventType(webhook?.EventType),
                ExternalListingId = webhook?.LoadId
            });
        }
        catch (JsonException ex)
        {
            logger.LogError(ex, "Error processing 123Loadboard webhook");
            return Task.FromResult(new LoadBoardWebhookResultDto
            {
                IsValid = false, EventType = LoadBoardWebhookEventType.Unknown, ErrorMessage = ex.Message
            });
        }
    }

    private bool CheckRateLimit()
    {
        lock (rateLimitLock)
        {
            var now = DateTime.UtcNow;

            if ((now - lastHourReset).TotalHours >= 1)
            {
                hourlySearchCount = 0;
                lastHourReset = now;
            }

            if ((now - lastDayReset).TotalDays >= 1)
            {
                dailySearchCount = 0;
                lastDayReset = now;
            }

            if (hourlySearchCount >= options.MaxSearchesPerHour)
            {
                logger.LogWarning("123Loadboard hourly rate limit reached: {Count}/{Max}",
                    hourlySearchCount, options.MaxSearchesPerHour);
                return false;
            }

            if (dailySearchCount >= options.MaxSearchesPerDay)
            {
                logger.LogWarning("123Loadboard daily rate limit reached: {Count}/{Max}",
                    dailySearchCount, options.MaxSearchesPerDay);
                return false;
            }

            return true;
        }
    }

    private void IncrementSearchCount()
    {
        lock (rateLimitLock)
        {
            hourlySearchCount++;
            dailySearchCount++;
        }
    }
}
