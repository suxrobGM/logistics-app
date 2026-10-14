using System.Net;
using System.Text;
using Logistics.Domain.Entities;
using Logistics.Domain.Primitives.Enums;
using Logistics.Infrastructure.Integrations.Eld;
using Logistics.Infrastructure.Integrations.Eld.Providers.Samsara;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Xunit;

namespace Logistics.Application.Tests.Eld;

public class SamsaraEldServiceTests
{
    private readonly RoutingHandler _http = new();
    private readonly SamsaraEldService _sut;

    public SamsaraEldServiceTests()
    {
        _sut = new SamsaraEldService(
            new HttpClient(_http),
            Options.Create(new EldOptions()),
            NullLogger<SamsaraEldService>.Instance);
    }

    #region Pagination

    [Fact]
    public async Task GetAllDriversAsync_TwoPages_FollowsEndCursor()
    {
        _http.Respond("/fleet/drivers", """
            { "data": [{ "id": "1", "name": "Ann" }], "pagination": { "endCursor": "abc", "hasNextPage": true } }
            """);
        _http.Respond("/fleet/drivers?after=abc", """
            { "data": [{ "id": "2", "name": "Bob" }], "pagination": { "endCursor": "", "hasNextPage": false } }
            """);
        Initialize();

        var drivers = (await _sut.GetAllDriversAsync()).ToList();

        Assert.Equal(["1", "2"], drivers.Select(d => d.ExternalDriverId));
        Assert.Equal(["/fleet/drivers", "/fleet/drivers?after=abc"], _http.Requested);
    }

    [Fact]
    public async Task GetAllDriversHosStatusAsync_LaterPageFails_ReturnsNothingRatherThanPartial()
    {
        _http.Respond("/fleet/hos/clocks", SamsaraMapperTests.HosClocksJson.Replace(
            "\"hasNextPage\": false", "\"hasNextPage\": true"));
        Initialize();

        var clocks = await _sut.GetAllDriversHosStatusAsync();

        Assert.Empty(clocks);
        Assert.Equal(2, _http.Requested.Count);
    }

    [Fact]
    public async Task GetDriverHosLogsAsync_SendsRfc3339Window()
    {
        _http.Respond(
            "/fleet/hos/logs?driverIds=88668&startTime=2026-10-06T00:00:00Z&endTime=2026-10-14T12:30:00Z",
            """{ "data": [], "pagination": { "endCursor": "", "hasNextPage": false } }""");
        Initialize();

        await _sut.GetDriverHosLogsAsync(
            "88668",
            new DateTime(2026, 10, 6, 0, 0, 0, DateTimeKind.Utc),
            new DateTime(2026, 10, 14, 12, 30, 0, DateTimeKind.Utc));

        Assert.Single(_http.Requested);
        Assert.Equal(0, _http.Unmatched);
    }

    #endregion

    #region Region

    [Theory]
    [InlineData(null, "api.samsara.com")]
    [InlineData("us", "api.samsara.com")]
    [InlineData("eu", "api.eu.samsara.com")]
    [InlineData("ca", "api.ca.samsara.com")]
    public async Task Initialize_Region_SelectsRegionalHost(string? region, string expectedHost)
    {
        _http.Respond("/fleet/vehicles", """{ "data": [], "pagination": { "endCursor": "", "hasNextPage": false } }""");
        Initialize(region);

        await _sut.GetAllVehiclesAsync();

        Assert.Equal(expectedHost, _http.LastHost);
    }

    #endregion

    private void Initialize(string? region = null) => _sut.Initialize(new EldProviderConfiguration
    {
        ProviderType = EldProviderType.Samsara,
        ApiKey = "samsara_api_token",
        Region = region
    });

    /// <summary>Answers by path and query; anything unscripted is a 500.</summary>
    private sealed class RoutingHandler : HttpMessageHandler
    {
        private readonly Dictionary<string, string> _responses = [];

        public List<string> Requested { get; } = [];
        public int Unmatched { get; private set; }
        public string? LastHost { get; private set; }

        public void Respond(string pathAndQuery, string json) => _responses[pathAndQuery] = json;

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken ct)
        {
            var key = request.RequestUri!.PathAndQuery;
            Requested.Add(key);
            LastHost = request.RequestUri.Host;

            if (!_responses.TryGetValue(key, out var json))
            {
                Unmatched++;
                return Task.FromResult(new HttpResponseMessage(HttpStatusCode.InternalServerError));
            }

            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(json, Encoding.UTF8, "application/json")
            });
        }
    }
}
