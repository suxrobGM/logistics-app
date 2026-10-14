using System.Net;
using Logistics.Infrastructure.Integrations.Common;
using Xunit;

namespace Logistics.Application.Tests.Eld;

public class RateLimitRetryHandlerTests
{
    [Fact]
    public async Task SendAsync_TooManyRequestsThenOk_RetriesAndReturnsOk()
    {
        var inner = new ScriptedHandler(TooManyRequests("0.01"), new HttpResponseMessage(HttpStatusCode.OK));
        using var client = new HttpClient(new RateLimitRetryHandler { InnerHandler = inner });

        var response = await client.GetAsync("https://api.samsara.com/fleet/drivers");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(2, inner.Calls);
    }

    [Fact]
    public async Task SendAsync_AlwaysTooManyRequests_GivesUpAfterThreeRetries()
    {
        var inner = new ScriptedHandler(
            TooManyRequests("0"), TooManyRequests("0"), TooManyRequests("0"), TooManyRequests("0"));
        using var client = new HttpClient(new RateLimitRetryHandler { InnerHandler = inner });

        var response = await client.GetAsync("https://api.samsara.com/fleet/drivers");

        Assert.Equal(HttpStatusCode.TooManyRequests, response.StatusCode);
        Assert.Equal(4, inner.Calls);
    }

    [Fact]
    public async Task SendAsync_ServerError_IsNotRetried()
    {
        var inner = new ScriptedHandler(new HttpResponseMessage(HttpStatusCode.InternalServerError));
        using var client = new HttpClient(new RateLimitRetryHandler { InnerHandler = inner });

        var response = await client.GetAsync("https://api.samsara.com/fleet/drivers");

        Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);
        Assert.Equal(1, inner.Calls);
    }

    private static HttpResponseMessage TooManyRequests(string retryAfter)
    {
        var response = new HttpResponseMessage(HttpStatusCode.TooManyRequests);
        response.Headers.TryAddWithoutValidation("Retry-After", retryAfter);
        return response;
    }

    private sealed class ScriptedHandler(params HttpResponseMessage[] responses) : HttpMessageHandler
    {
        public int Calls { get; private set; }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken ct) =>
            Task.FromResult(responses[Calls++]);
    }
}
