using System.Net;
using System.Text;
using System.Text.Json;
using Logistics.Application.Abstractions.LoadBoard;
using Logistics.Domain.Entities;
using Logistics.Domain.Primitives.Enums;
using Logistics.Infrastructure.Integrations.LoadBoard;
using Logistics.Infrastructure.Integrations.LoadBoard.Providers.Dat;
using Logistics.Shared.Models;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Xunit;

namespace Logistics.Application.Tests.LoadBoard;

public class DatLoadBoardServiceTests
{
    /// <summary>
    /// One match in the shape DAT's v1 search returned in a captured production response, with
    /// the broker's identity replaced.
    /// </summary>
    private const string MatchesJson = """
        [{
          "matchId": "LS41mA3x", "postingId": "LS41mA3x", "status": "ACTIVE", "version": "V1",
          "createdWhen": "2021-06-26T14:35:36.259Z",
          "credit": { "scoreUpdatedWhen": "2021-06-25T16:42:52.000Z", "score": 95, "daysToPay": 18 },
          "tripLength": { "distanceMiles": 963, "type": "ROAD" },
          "origin": { "point": { "city": "Victorville", "state": "CA", "latitude": 34.53611, "longitude": -117.29028,
                                 "deadhead": { "miles": 85, "type": "ROAD" } } },
          "destination": { "point": { "city": "Woodburn", "state": "OR", "latitude": 45.14389, "longitude": -122.85417 } },
          "comments": [{ "comment": "call for details" }],
          "poster": { "company": "Example Brokerage Inc", "city": "Milford", "state": "OH",
                      "contact": { "email": "loads@broker.example", "phone": { "number": "5135550100", "extension": "12" } } },
          "specs": { "requiredEquipmentType": "V", "weightInPounds": 44600, "requiredCapacity": "FULL", "lengthInFeet": 53 },
          "availability": { "earliest": "2021-06-27T15:00:00.000Z", "latest": "2021-06-28T06:59:59.999Z" },
          "rate": { "rateBasedOn": "FLAT", "baseRate": { "amount": 4600, "currency": "USD" } }
        }]
        """;

    private readonly ScriptedHandler _handler = new();
    private readonly DatLoadBoardService _sut;

    private readonly LoadBoardConfiguration _config = new()
    {
        ProviderType = LoadBoardProviderType.Dat,
        ApiKey = "svc@carrier.example",
        ApiSecret = "s3cret",
        UserEmail = "dispatch@carrier.example"
    };

    public DatLoadBoardServiceTests()
    {
        var options = Options.Create(new LoadBoardOptions { Dat = new DatOptions() });
        _sut = new DatLoadBoardService(new HttpClient(_handler), options, NullLogger<DatLoadBoardService>.Instance);
    }

    #region Auth

    [Fact]
    public async Task AcquireToken_ExchangesServiceAccountThenUser_KeepsOrgTokenAsRefresh()
    {
        _handler.Respond(HttpMethod.Post, "/access/v1/token/organization",
            """{"accessToken":"org-token","expiresWhen":"2026-10-14T22:00:00.000Z"}""");
        _handler.Respond(HttpMethod.Post, "/access/v1/token/user",
            """{"accessToken":"user-token","expiresWhen":"2026-10-14T21:30:00.000Z"}""");

        var token = await _sut.AcquireTokenAsync(_config);

        Assert.Equal("user-token", token.AccessToken);
        Assert.Equal("org-token", token.RefreshToken);
        Assert.Equal(new DateTime(2026, 10, 14, 21, 30, 0, DateTimeKind.Utc), token.ExpiresAt);

        var org = _handler.Requests[0];
        Assert.Equal("https://identity.api.dat.com/access/v1/token/organization", org.Url);
        Assert.Null(org.Authorization);
        Assert.Equal("svc@carrier.example", org.Body!.RootElement.GetProperty("username").GetString());
        Assert.Equal("s3cret", org.Body.RootElement.GetProperty("password").GetString());

        var user = _handler.Requests[1];
        Assert.Equal("Bearer org-token", user.Authorization);
        Assert.Equal("dispatch@carrier.example", user.Body!.RootElement.GetProperty("username").GetString());
    }

    [Fact]
    public async Task RefreshToken_MintsUserTokenFromStoredOrgToken_WithoutServiceAccountLogin()
    {
        _config.RefreshToken = "org-token";
        _handler.Respond(HttpMethod.Post, "/access/v1/token/user",
            """{"accessToken":"user-2","expiresWhen":"2026-10-14T21:30:00.000Z"}""");

        var token = await _sut.RefreshTokenAsync(_config);

        Assert.Equal("user-2", token!.AccessToken);
        Assert.Null(token.RefreshToken);
        var request = Assert.Single(_handler.Requests);
        Assert.Equal("Bearer org-token", request.Authorization);
    }

    [Fact]
    public async Task AcquireToken_NoUserEmail_FailsWithAPlainInstruction()
    {
        _config.UserEmail = null;
        _handler.Respond(HttpMethod.Post, "/access/v1/token/organization", """{"accessToken":"org-token"}""");

        var ex = await Assert.ThrowsAsync<LoadBoardApiException>(() => _sut.AcquireTokenAsync(_config));

        Assert.Contains("DAT One user email", ex.Message);
    }

    [Fact]
    public async Task AcquireToken_RejectedServiceAccount_ThrowsUnauthorizedWithVendorText()
    {
        _handler.Respond(HttpMethod.Post, "/access/v1/token/organization",
            """{"message":"invalid username or password"}""", HttpStatusCode.Unauthorized);

        var ex = await Assert.ThrowsAsync<LoadBoardApiException>(() => _sut.AcquireTokenAsync(_config));

        Assert.True(ex.IsUnauthorized);
        Assert.Contains("invalid username or password", ex.Message);
    }

    #endregion

    #region Search

    [Fact]
    public async Task SearchLoads_CreatesQueryThenReadsMatches_WithUserToken()
    {
        Initialize();
        _handler.Respond(HttpMethod.Post, "/search/v1/loads", """{"id":"q-1"}""");
        _handler.Respond(HttpMethod.Get, "/search/v1/loads/q-1/matches?limit=50", MatchesJson);

        var listings = await _sut.SearchLoadsAsync(new LoadBoardSearchCriteria
        {
            Origin = new LoadBoardPlace { City = "Victorville", State = "ca" },
            OriginRadius = 100,
            EquipmentTypes = [LoadBoardEquipmentType.Van, LoadBoardEquipmentType.Tanker],
            PickupDateStart = new DateTime(2021, 6, 27, 0, 0, 0, DateTimeKind.Utc),
            MaxResults = 50
        });

        var create = _handler.Requests[0];
        Assert.Equal("https://freight.api.prod.dat.com/search/v1/loads", create.Url);
        Assert.Equal("Bearer user-token", create.Authorization);
        var criteria = create.Body!.RootElement.GetProperty("criteria");
        var origin = criteria.GetProperty("origin").GetProperty("point");
        Assert.Equal("Victorville", origin.GetProperty("city").GetString());
        Assert.Equal("CA", origin.GetProperty("state").GetString());
        Assert.Equal(100, origin.GetProperty("deadhead").GetProperty("miles").GetInt32());
        Assert.Equal(["V", "TS", "TA", "TN"],
            criteria.GetProperty("desiredSpecs").GetProperty("equipmentTypes").EnumerateArray().Select(e => e.GetString()));
        Assert.Equal("2021-06-27T00:00:00Z", criteria.GetProperty("availability").GetProperty("earliest").GetString());
        Assert.False(criteria.TryGetProperty("destination", out _));

        Assert.Equal("Bearer user-token", _handler.Requests[1].Authorization);
        Assert.Single(listings);
    }

    [Fact]
    public async Task SearchLoads_MapsMatchWithoutInventingAddressData()
    {
        Initialize();
        _handler.Respond(HttpMethod.Post, "/search/v1/loads", """{"id":"q-1"}""");
        _handler.Respond(HttpMethod.Get, "/search/v1/loads/q-1/matches?limit=100", MatchesJson);

        var listing = Assert.Single(await _sut.SearchLoadsAsync(Criteria()));

        Assert.Equal("LS41mA3x", listing.ExternalListingId);
        Assert.Equal("Victorville", listing.OriginAddress.City);
        Assert.Equal("CA", listing.OriginAddress.State);
        Assert.Equal("", listing.OriginAddress.Line1);
        Assert.Equal("", listing.OriginAddress.ZipCode);
        Assert.Equal("US", listing.OriginAddress.Country);
        Assert.Equal(45.14389, listing.DestinationLocation.Latitude);
        Assert.Equal(4600m, listing.TotalRate);
        Assert.Equal(4.78m, listing.RatePerMile);
        Assert.Equal(963, listing.Distance);
        Assert.Equal(44600, listing.Weight);
        Assert.Equal(53, listing.Length);
        Assert.Equal(LoadBoardEquipmentType.Van, listing.EquipmentType);
        Assert.Equal("Example Brokerage Inc", listing.BrokerName);
        Assert.Equal("5135550100 x12", listing.BrokerPhone);
        Assert.Equal("loads@broker.example", listing.BrokerEmail);
        Assert.Equal(95, listing.BrokerCreditScore);
        Assert.Equal(18, listing.BrokerDaysToPay);
        Assert.Equal(new DateTime(2021, 6, 27, 15, 0, 0, DateTimeKind.Utc), listing.PickupDateStart);
        Assert.Null(listing.ExpiresAt);
    }

    [Fact]
    public async Task SearchLoads_StateOnlyDestination_FiltersMatchesLocally()
    {
        Initialize();
        _handler.Respond(HttpMethod.Post, "/search/v1/loads", """{"id":"q-1"}""");
        _handler.Respond(HttpMethod.Get, "/search/v1/loads/q-1/matches?limit=100", MatchesJson);

        var listings = await _sut.SearchLoadsAsync(Criteria() with { Destination = new LoadBoardPlace { State = "WA" } });

        Assert.Empty(listings);
        Assert.False(_handler.Requests[0].Body!.RootElement.GetProperty("criteria").TryGetProperty("destination", out _));
    }

    [Fact]
    public async Task SearchLoads_NoOriginCity_FailsBeforeCallingDat()
    {
        Initialize();

        var ex = await Assert.ThrowsAsync<LoadBoardApiException>(() =>
            _sut.SearchLoadsAsync(new LoadBoardSearchCriteria { Origin = new LoadBoardPlace { State = "TX" } }));

        Assert.Contains("origin city and state", ex.Message);
        Assert.Empty(_handler.Requests);
    }

    [Fact]
    public async Task SearchLoads_VendorError_SurfacesStatusAndBody()
    {
        Initialize();
        _handler.Respond(HttpMethod.Post, "/search/v1/loads", """{"message":"origin not found"}""", HttpStatusCode.BadRequest);

        var ex = await Assert.ThrowsAsync<LoadBoardApiException>(() => _sut.SearchLoadsAsync(Criteria()));

        Assert.Equal(HttpStatusCode.BadRequest, ex.StatusCode);
        Assert.Contains("400", ex.Message);
        Assert.Contains("origin not found", ex.Message);
    }

    #endregion

    #region Truck posts

    [Fact]
    public async Task PostTruck_SendsDatEquipmentCodeAndLane_ReturnsPostId()
    {
        Initialize();
        _handler.Respond(HttpMethod.Post, "/posting/v2/trucks", """{"id":"P-9"}""");

        var result = await _sut.PostTruckAsync(new PostTruckRequest
        {
            TruckId = Guid.NewGuid(),
            Origin = new LoadBoardPlace { City = "Dallas", State = "TX", PostalCode = "75201" },
            AvailableFrom = new DateTime(2026, 10, 15, 8, 0, 0, DateTimeKind.Utc),
            EquipmentType = LoadBoardEquipmentType.StepDeck
        });

        Assert.Equal("P-9", result.ExternalPostId);
        var body = _handler.Requests[0].Body!.RootElement;
        Assert.Equal("SD", body.GetProperty("freight").GetProperty("equipmentType").GetString());
        var origin = body.GetProperty("lane").GetProperty("origin");
        Assert.Equal("TX", origin.GetProperty("stateProv").GetString());
        Assert.Equal("75201", origin.GetProperty("postalCode").GetString());
        Assert.False(body.GetProperty("lane").TryGetProperty("destination", out _));
    }

    [Fact]
    public async Task RemoveTruckPost_AlreadyGoneOnDat_Succeeds()
    {
        Initialize();
        _handler.Respond(HttpMethod.Delete, "/posting/v2/trucks/?id=P-9", "", HttpStatusCode.NotFound);

        await _sut.RemoveTruckPostAsync("P-9");
    }

    [Fact]
    public async Task GetPostedTrucks_ReadsIdsAndExpiry()
    {
        Initialize();
        _handler.Respond(HttpMethod.Get, "/posting/v2/trucks",
            """[{"id":"P-1","exposure":{"endWhen":"2026-10-16T00:00:00.000Z"}},{"id":"P-2"}]""");

        var posts = await _sut.GetPostedTrucksAsync();

        Assert.NotNull(posts);
        Assert.Equal(["P-1", "P-2"], posts.Select(p => p.ExternalPostId));
        Assert.Equal(new DateTime(2026, 10, 16, 0, 0, 0, DateTimeKind.Utc), posts[0].ExpiresAt);
    }

    #endregion

    #region Market rates

    [Fact]
    public async Task GetMarketRates_BatchesPriceableLanesAndAlignsResults()
    {
        Initialize();
        _handler.Respond(HttpMethod.Post, "/linehaulrates/v1/lookups", """
            {"rateResponses":[
              {"response":{"rate":{"perMile":{"rateUsd":2.61,"lowUsd":2.2,"highUsd":3.0},"perTrip":{"rateUsd":2513},"mileage":963}}},
              {"response":{}}
            ]}
            """);

        var rates = await _sut.GetMarketRatesAsync([
            new MarketRateLane("Victorville", "CA", "Woodburn", "OR", LoadBoardEquipmentType.Van),
            new MarketRateLane("Dallas", "TX", "Tulsa", "OK", LoadBoardEquipmentType.PowerOnly),
            new MarketRateLane("Dallas", "TX", "Houston", "TX", LoadBoardEquipmentType.StepDeck)
        ]);

        Assert.Equal(2.61m, rates[0]!.RatePerMile);
        Assert.Null(rates[1]);
        Assert.Null(rates[2]);

        var lookups = _handler.Requests[0].Body!.RootElement.EnumerateArray().ToList();
        Assert.Equal(["VAN", "FLATBED"], lookups.Select(l => l.GetProperty("equipment").GetString()));
        Assert.Equal("CA", lookups[0].GetProperty("origin").GetProperty("stateOrProvince").GetString());
    }

    #endregion

    #region Equipment codes

    [Theory]
    [InlineData("V", LoadBoardEquipmentType.Van)]
    [InlineData("VZ", LoadBoardEquipmentType.Van)]
    [InlineData("RA", LoadBoardEquipmentType.Reefer)]
    [InlineData("FT", LoadBoardEquipmentType.Flatbed)]
    [InlineData("FD", LoadBoardEquipmentType.FlatbedOrStepDeck)]
    [InlineData("SN", LoadBoardEquipmentType.Conestoga)]
    [InlineData("TA", LoadBoardEquipmentType.Tanker)]
    [InlineData("sb", LoadBoardEquipmentType.BoxTruck)]
    [InlineData("AC", LoadBoardEquipmentType.AutoCarrier)]
    public void FromCode_KnownCodes_MapToOneType(string code, LoadBoardEquipmentType expected)
    {
        Assert.Equal(expected, DatEquipment.FromCode(code));
    }

    [Fact]
    public void FromCode_UnknownCode_IsNullNotAGuess()
    {
        Assert.Null(DatEquipment.FromCode("ZZ"));
    }

    [Fact]
    public void ToPostingCode_EveryEquipmentType_HasACodeThatMapsBack()
    {
        foreach (var type in Enum.GetValues<LoadBoardEquipmentType>())
        {
            Assert.Equal(type, DatEquipment.FromCode(DatEquipment.ToPostingCode(type)));
        }
    }

    #endregion

    private void Initialize()
    {
        _config.AccessToken = "user-token";
        _sut.Initialize(_config);
    }

    private static LoadBoardSearchCriteria Criteria() => new()
    {
        Origin = new LoadBoardPlace { City = "Victorville", State = "CA" }
    };

    private sealed record RecordedRequest(HttpMethod Method, string Url, string? Authorization, JsonDocument? Body);

    /// <summary>
    /// Answers by method and path-and-query; anything unscripted is a 500 so a wrong URL fails loudly.
    /// </summary>
    private sealed class ScriptedHandler : HttpMessageHandler
    {
        private readonly Dictionary<(HttpMethod, string), (string Body, HttpStatusCode Status)> _responses = [];

        public List<RecordedRequest> Requests { get; } = [];

        public void Respond(HttpMethod method, string pathAndQuery, string body, HttpStatusCode status = HttpStatusCode.OK) =>
            _responses[(method, pathAndQuery)] = (body, status);

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken ct)
        {
            var body = request.Content is null ? null : JsonDocument.Parse(await request.Content.ReadAsStringAsync(ct));
            Requests.Add(new RecordedRequest(
                request.Method, request.RequestUri!.ToString(), request.Headers.Authorization?.ToString(), body));

            return _responses.TryGetValue((request.Method, request.RequestUri.PathAndQuery), out var response)
                ? new HttpResponseMessage(response.Status)
                {
                    Content = new StringContent(response.Body, Encoding.UTF8, "application/json")
                }
                : new HttpResponseMessage(HttpStatusCode.InternalServerError);
        }
    }
}
