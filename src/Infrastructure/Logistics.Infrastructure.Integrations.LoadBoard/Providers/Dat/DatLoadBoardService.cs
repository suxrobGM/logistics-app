using System.Net;
using Logistics.Application.Abstractions.LoadBoard;
using Logistics.Domain.Entities;
using Logistics.Domain.Primitives.Enums;
using Logistics.Shared.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Logistics.Infrastructure.Integrations.LoadBoard.Providers.Dat;

/// <summary>
/// DAT (DAT One) through DAT's public freight APIs.
/// <para>
/// Auth is two-step: the carrier's service account (email + password) gets an organization
/// token, and that token mints a user token for one DAT One seat (<see cref="LoadBoardConfiguration.UserEmail"/>).
/// Every freight call uses the user token. The organization token is kept as the refresh
/// credential, so renewing a user token skips the service-account login.
/// </para>
/// <para>DAT has no webhooks and no booking API: searches and post reconciliation are pulled.</para>
/// </summary>
internal sealed class DatLoadBoardService(
    HttpClient httpClient,
    IOptions<LoadBoardOptions> options,
    ILogger<DatLoadBoardService> logger)
    : ILoadBoardProviderService
{
    private const string Vendor = "DAT";

    /// <summary>
    /// Used when DAT omits <c>expiresWhen</c>; short enough that a token is renewed early rather
    /// than late.
    /// </summary>
    private static readonly TimeSpan FallbackTokenLifetime = TimeSpan.FromMinutes(30);

    private readonly DatOptions _options = options.Value.Dat ?? new DatOptions();
    private LoadBoardConfiguration? _configuration;

    public LoadBoardProviderType ProviderType => LoadBoardProviderType.Dat;

    public void Initialize(LoadBoardConfiguration configuration)
    {
        _configuration = configuration;
    }

    public async Task<OAuthTokenResultDto> AcquireTokenAsync(LoadBoardConfiguration configuration, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(configuration.ApiSecret))
        {
            throw new LoadBoardApiException("Enter the DAT service account password.");
        }

        var organization = await RequestTokenAsync(
            "/access/v1/token/organization",
            new DatOrganizationTokenRequest(configuration.ApiKey, configuration.ApiSecret),
            bearerToken: null,
            ct);

        var user = await RequestUserTokenAsync(configuration, organization.AccessToken!, ct);
        return user with { RefreshToken = organization.AccessToken };
    }

    public async Task<OAuthTokenResultDto?> RefreshTokenAsync(LoadBoardConfiguration configuration, CancellationToken ct = default)
    {
        return string.IsNullOrEmpty(configuration.RefreshToken)
            ? null
            : await RequestUserTokenAsync(configuration, configuration.RefreshToken, ct);
    }

    public async Task<IReadOnlyList<LoadBoardListingDto>> SearchLoadsAsync(
        LoadBoardSearchCriteria criteria, CancellationToken ct = default)
    {
        var request = DatSearchRequestBuilder.Build(criteria, _options.MaxPostingAgeMinutes);
        var created = await httpClient.SendJsonAsync<DatSearchCreatedResponse>(
            HttpMethod.Post, $"{_options.SearchBaseUrl}/search/v1/loads", request, UserToken, Vendor, ct);

        if (string.IsNullOrEmpty(created.Id))
        {
            throw new LoadBoardApiException("DAT accepted the search but returned no search id.");
        }

        var limit = Math.Clamp(criteria.MaxResults, 1, 1000);
        var matches = await httpClient.SendJsonAsync<List<DatMatch>>(
            HttpMethod.Get,
            $"{_options.SearchBaseUrl}/search/v1/loads/{Uri.EscapeDataString(created.Id)}/matches?limit={limit}",
            null, UserToken, Vendor, ct);

        var listings = matches
            .Select(DatMapper.ToListingDto)
            .OfType<LoadBoardListingDto>()
            .Where(l => DatSearchRequestBuilder.MatchesLocalFilters(l, criteria))
            .ToList();

        logger.LogInformation("DAT search {SearchId} returned {Count} matches", created.Id, listings.Count);
        return listings;
    }

    public async Task<PostTruckResultDto> PostTruckAsync(PostTruckRequest request, CancellationToken ct = default)
    {
        var posting = await httpClient.SendJsonAsync<DatPosting>(
            HttpMethod.Post, $"{_options.FreightBaseUrl}/posting/v2/trucks", ToPostingRequest(request), UserToken, Vendor, ct);

        if (string.IsNullOrEmpty(posting.Id))
        {
            throw new LoadBoardApiException("DAT accepted the truck post but returned no post id.");
        }

        return new PostTruckResultDto { ExternalPostId = posting.Id, ExpiresAt = posting.Exposure?.EndWhen };
    }

    public async Task RemoveTruckPostAsync(string externalPostId, CancellationToken ct = default)
    {
        try
        {
            await httpClient.SendJsonAsync(
                HttpMethod.Delete,
                $"{_options.FreightBaseUrl}/posting/v2/trucks/?id={Uri.EscapeDataString(externalPostId)}",
                null, UserToken, Vendor, ct);
        }
        catch (LoadBoardApiException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
        {
            logger.LogInformation("DAT truck post {PostId} was already gone", externalPostId);
        }
    }

    public async Task<IReadOnlyList<ProviderTruckPost>?> GetPostedTrucksAsync(CancellationToken ct = default)
    {
        var postings = await httpClient.SendJsonAsync<List<DatPosting>>(
            HttpMethod.Get, $"{_options.FreightBaseUrl}/posting/v2/trucks", null, UserToken, Vendor, ct);

        return postings
            .Where(p => !string.IsNullOrEmpty(p.Id))
            .Select(p => new ProviderTruckPost(p.Id!, p.Exposure?.EndWhen))
            .ToList();
    }

    public async Task<IReadOnlyList<MarketRate?>> GetMarketRatesAsync(
        IReadOnlyList<MarketRateLane> lanes, CancellationToken ct = default)
    {
        var rates = new MarketRate?[lanes.Count];

        // RateView answers a lookup array in request order; lanes it cannot price are left out.
        var priced = lanes
            .Select((lane, index) => (lane, index, equipment: DatEquipment.ToRateViewEquipment(lane.EquipmentType)))
            .Where(x => x.equipment is not null)
            .ToList();

        if (priced.Count == 0)
        {
            return rates;
        }

        var request = priced.Select(x => new DatRateLookupRequest
        {
            Origin = new DatRatePlace(x.lane.OriginCity, x.lane.OriginState),
            Destination = new DatRatePlace(x.lane.DestinationCity, x.lane.DestinationState),
            Equipment = x.equipment!
        }).ToList();

        var response = await httpClient.SendJsonAsync<DatRateLookupResponse>(
            HttpMethod.Post, $"{_options.AnalyticsBaseUrl}/linehaulrates/v1/lookups", request, UserToken, Vendor, ct);

        var results = response.RateResponses ?? [];
        for (var i = 0; i < priced.Count && i < results.Count; i++)
        {
            rates[priced[i].index] = ToMarketRate(results[i].Response?.Rate);
        }

        return rates;
    }

    private string UserToken =>
        _configuration?.AccessToken ?? throw new InvalidOperationException("DAT provider used before Initialize.");

    private async Task<OAuthTokenResultDto> RequestUserTokenAsync(
        LoadBoardConfiguration configuration, string organizationToken, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(configuration.UserEmail))
        {
            throw new LoadBoardApiException(
                "Add the DAT One user email to the DAT connection: DAT issues API access per DAT One user.");
        }

        return await RequestTokenAsync(
            "/access/v1/token/user", new DatUserTokenRequest(configuration.UserEmail), organizationToken, ct);
    }

    private async Task<OAuthTokenResultDto> RequestTokenAsync(
        string path, object body, string? bearerToken, CancellationToken ct)
    {
        var token = await httpClient.SendJsonAsync<DatTokenResponse>(
            HttpMethod.Post, $"{_options.IdentityBaseUrl}{path}", body, bearerToken, Vendor, ct);

        if (string.IsNullOrEmpty(token.AccessToken))
        {
            throw new LoadBoardApiException("DAT returned no access token.");
        }

        return new OAuthTokenResultDto
        {
            AccessToken = token.AccessToken,
            ExpiresAt = token.ExpiresWhen?.ToUniversalTime() ?? DateTime.UtcNow + FallbackTokenLifetime
        };
    }

    private static DatTruckPostingRequest ToPostingRequest(PostTruckRequest request) => new()
    {
        Freight = new DatPostingFreight
        {
            EquipmentType = DatEquipment.ToPostingCode(request.EquipmentType),
            LengthFeet = request.MaxLength,
            WeightPounds = request.MaxWeight
        },
        Lane = new DatPostingLane
        {
            Origin = ToPostingPlace(request.Origin),
            Destination = request.Destination is null ? null : ToPostingPlace(request.Destination)
        },
        Exposure = new DatPostingExposure
        {
            EarliestAvailabilityWhen = request.AvailableFrom.ToUniversalTime(),
            LatestAvailabilityWhen = request.AvailableTo?.ToUniversalTime()
        },
        ReferenceId = request.TruckId.ToString("N")[..8].ToUpperInvariant()
    };

    private static DatPostingPlace ToPostingPlace(LoadBoardPlace place) => new()
    {
        City = place.City,
        StateProv = place.State,
        PostalCode = place.PostalCode
    };

    private static MarketRate? ToMarketRate(DatRateValues? rate)
    {
        var perMile = rate?.PerMile?.RateUsd;
        var perTrip = rate?.PerTrip?.RateUsd;
        if (perMile is null && perTrip is not null && rate?.Mileage is > 0)
        {
            perMile = Math.Round(perTrip.Value / (decimal)rate.Mileage.Value, 2);
        }

        return perMile is { } value
            ? new MarketRate(value, perTrip, rate?.PerMile?.LowUsd, rate?.PerMile?.HighUsd)
            : null;
    }
}
