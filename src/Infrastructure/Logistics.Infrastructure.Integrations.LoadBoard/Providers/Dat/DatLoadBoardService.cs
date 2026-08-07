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

namespace Logistics.Infrastructure.Integrations.LoadBoard.Providers.Dat;

/// <summary>
///     DAT Load Board provider implementation.
///     Integrates with DAT Freight &amp; Analytics API for searching loads and posting trucks.
///     Authentication: Bearer Token (Service Account or User Account)
///     Base URL: https://freight.api.dat.com
/// </summary>
internal class DatLoadBoardService(
    HttpClient httpClient,
    IHttpClientFactory httpClientFactory,
    IOptions<LoadBoardOptions> options,
    ILogger<DatLoadBoardService> logger)
    : ILoadBoardProviderService
{
    private readonly DatOptions options = options.Value.Dat ?? new DatOptions();

    public LoadBoardProviderType ProviderType => LoadBoardProviderType.Dat;

    public bool RequiresOAuthToken => true;

    public void Initialize(LoadBoardConfiguration configuration)
    {
        httpClient.BaseAddress = new Uri(options.BaseUrl);

        if (!string.IsNullOrEmpty(configuration.AccessToken))
        {
            httpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", configuration.AccessToken);
        }

        logger.LogInformation("Initialized DAT Load Board provider");
    }

    public async Task<bool> ValidateCredentialsAsync(string apiKey, string? apiSecret)
    {
        return await AcquireTokenAsync(apiKey, apiSecret) != null;
    }

    public async Task<OAuthTokenResultDto?> AcquireTokenAsync(string apiKey, string? apiSecret)
    {
        try
        {
            var authClient = httpClientFactory.CreateClient();
            var response = await authClient.PostAsJsonAsync(options.AuthUrl,
                new { clientId = apiKey, clientSecret = apiSecret });
            if (!response.IsSuccessStatusCode)
            {
                logger.LogWarning("DAT token acquisition failed: {StatusCode}", response.StatusCode);
                return null;
            }

            var token = await response.Content.ReadFromJsonAsync<DatTokenResponse>();
            if (string.IsNullOrEmpty(token?.AccessToken))
            {
                logger.LogWarning("DAT token response contained no access token");
                return null;
            }

            return new OAuthTokenResultDto
            {
                AccessToken = token.AccessToken,
                ExpiresAt = token.ExpiresWhen ?? DateTime.UtcNow.AddMinutes(50)
            };
        }
        catch (Exception ex) when (ex is HttpRequestException or JsonException or TaskCanceledException)
        {
            logger.LogError(ex, "Error acquiring DAT token");
            return null;
        }
    }

    public Task<OAuthTokenResultDto?> RefreshTokenAsync(string refreshToken)
    {
        // DAT has no refresh grant; callers fall through to AcquireTokenAsync (client credentials).
        return Task.FromResult<OAuthTokenResultDto?>(null);
    }

    public async Task<IEnumerable<LoadBoardListingDto>> SearchLoadsAsync(LoadBoardSearchCriteria criteria)
    {
        logger.LogInformation("Searching DAT loads: Origin={Origin}, Dest={Dest}",
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
            pickupDate = new
            {
                start = criteria.PickupDateStart?.ToString("yyyy-MM-dd"),
                end = criteria.PickupDateEnd?.ToString("yyyy-MM-dd")
            },
            equipmentTypes = criteria.EquipmentTypes,
            limit = criteria.MaxResults
        };

        var result = await httpClient.TryPostAsJsonAsync<object, DatSearchResponse>(
            "/freight/v3/loads/search", searchRequest, logger, "DAT search loads");

        return result.Value?.Loads?.Select(DatMapper.ToListingDto) ?? [];
    }

    public async Task<LoadBoardListingDto?> GetLoadDetailsAsync(string externalListingId)
    {
        var load = await httpClient.TryGetFromJsonAsync<DatLoad>(
            $"/freight/v3/loads/{externalListingId}", logger, $"DAT get load {externalListingId}");

        return load != null ? DatMapper.ToListingDto(load) : null;
    }

    public async Task<LoadBoardBookingResultDto> BookLoadAsync(string externalListingId,
        LoadBoardBookingRequest request)
    {
        logger.LogInformation("Booking DAT load {ListingId} for truck {TruckId}", externalListingId, request.TruckId);

        var bookRequest = new
        {
            loadId = externalListingId, truckId = request.TruckId.ToString(), notes = request.Notes
        };

        var result = await httpClient.TryPostAsJsonAsync<object, DatBookingResponse>(
            $"/freight/v3/loads/{externalListingId}/book", bookRequest, logger, $"DAT book load {externalListingId}");

        return result.IsSuccess
            ? new LoadBoardBookingResultDto { Success = true, ExternalConfirmationId = result.Value?.ConfirmationId }
            : new LoadBoardBookingResultDto { Success = false, ErrorMessage = $"DAT booking failed: {result.ErrorBody}" };
    }

    public Task<bool> CancelBookingAsync(string externalListingId, string? reason)
    {
        return httpClient.TryPostAsync(
            $"/freight/v3/loads/{externalListingId}/cancel", new { reason }, logger, $"DAT cancel booking {externalListingId}");
    }

    public async Task<PostTruckResultDto> PostTruckAsync(PostTruckRequest request)
    {
        logger.LogInformation("Posting truck {TruckId} to DAT", request.TruckId);

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
                    city = request.DestinationPreference.City,
                    state = request.DestinationPreference.State,
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

        var result = await httpClient.TryPostAsJsonAsync<object, DatPostTruckResponse>(
            "/freight/v3/trucks", postRequest, logger, $"DAT post truck {request.TruckId}");

        return result.IsSuccess
            ? new PostTruckResultDto
            {
                Success = true, ExternalPostId = result.Value?.PostId, ExpiresAt = result.Value?.ExpiresAt
            }
            : new PostTruckResultDto { Success = false, ErrorMessage = $"DAT post truck failed: {result.ErrorBody}" };
    }

    public Task<bool> UpdateTruckPostAsync(string externalPostId, PostTruckRequest request)
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

        return httpClient.TryPutAsync(
            $"/freight/v3/trucks/{externalPostId}", updateRequest, logger, $"DAT update truck post {externalPostId}");
    }

    public Task<bool> RemoveTruckPostAsync(string externalPostId)
    {
        return httpClient.TryDeleteAsync(
            $"/freight/v3/trucks/{externalPostId}", logger, $"DAT remove truck post {externalPostId}");
    }

    public async Task<IEnumerable<PostedTruckDto>> GetPostedTrucksAsync()
    {
        var result = await httpClient.TryGetFromJsonAsync<DatTrucksResponse>(
            "/freight/v3/trucks", logger, "DAT get posted trucks");

        return result?.Trucks?.Select(DatMapper.ToPostedTruckDto) ?? [];
    }

    public Task<LoadBoardWebhookResultDto> ProcessWebhookAsync(string payload, string? signature,
        string? webhookSecret)
    {
        if (!string.IsNullOrEmpty(webhookSecret))
        {
            if (!WebhookSignature.VerifyHmacSha256(payload, signature, webhookSecret))
            {
                logger.LogWarning("Rejected DAT webhook with invalid signature");
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
            logger.LogWarning("DAT webhook processed without signature verification - no webhook secret configured");
        }

        try
        {
            var webhook = JsonSerializer.Deserialize<DatWebhookPayload>(payload);
            return Task.FromResult(new LoadBoardWebhookResultDto
            {
                IsValid = true,
                EventType = DatMapper.MapWebhookEventType(webhook?.EventType),
                ExternalListingId = webhook?.LoadId
            });
        }
        catch (JsonException ex)
        {
            logger.LogError(ex, "Error processing DAT webhook");
            return Task.FromResult(new LoadBoardWebhookResultDto
            {
                IsValid = false, EventType = LoadBoardWebhookEventType.Unknown, ErrorMessage = ex.Message
            });
        }
    }
}
