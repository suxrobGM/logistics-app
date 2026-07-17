using System.Net;
using System.Text;
using Logistics.Infrastructure.Integrations.LoadBoard.Credit;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Xunit;

namespace Logistics.Application.Tests.LoadBoard;

public class FmcsaClientTests
{
    private static FmcsaClient CreateClient(string? responseBody, HttpStatusCode statusCode = HttpStatusCode.OK, string? webKey = "test-key")
    {
        var handler = new StubHttpMessageHandler(responseBody, statusCode);
        return new FmcsaClient(
            new HttpClient(handler),
            Options.Create(new FmcsaOptions { WebKey = webKey }),
            NullLogger<FmcsaClient>.Instance);
    }

    [Fact]
    public async Task GetAuthorityActive_AllowedToOperateY_ReturnsTrue()
    {
        var client = CreateClient("""
            {"content":[{"carrier":{"legalName":"Test Broker LLC","allowedToOperate":"Y","dotNumber":1234567}}]}
            """);

        var result = await client.GetAuthorityActiveAsync("123456");

        Assert.True(result);
    }

    [Fact]
    public async Task GetAuthorityActive_AllowedToOperateN_ReturnsFalse()
    {
        var client = CreateClient("""
            {"content":[{"carrier":{"legalName":"Shady Broker LLC","allowedToOperate":"N"}}]}
            """);

        var result = await client.GetAuthorityActiveAsync("123456");

        Assert.False(result);
    }

    [Fact]
    public async Task GetAuthorityActive_UnknownDocket_ReturnsNull()
    {
        var client = CreateClient("""{"content":[]}""");

        var result = await client.GetAuthorityActiveAsync("999999");

        Assert.Null(result);
    }

    [Fact]
    public async Task GetAuthorityActive_ApiError_ReturnsNull()
    {
        var client = CreateClient(null, HttpStatusCode.InternalServerError);

        var result = await client.GetAuthorityActiveAsync("123456");

        Assert.Null(result);
    }

    [Fact]
    public async Task GetAuthorityActive_NoWebKeyConfigured_ReturnsNullWithoutCalling()
    {
        var handler = new StubHttpMessageHandler("{}", HttpStatusCode.OK);
        var client = new FmcsaClient(
            new HttpClient(handler),
            Options.Create(new FmcsaOptions { WebKey = null }),
            NullLogger<FmcsaClient>.Instance);

        var result = await client.GetAuthorityActiveAsync("123456");

        Assert.Null(result);
        Assert.Equal(0, handler.CallCount);
    }

    private sealed class StubHttpMessageHandler(string? responseBody, HttpStatusCode statusCode) : HttpMessageHandler
    {
        public int CallCount { get; private set; }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            CallCount++;
            var response = new HttpResponseMessage(statusCode);
            if (responseBody is not null)
            {
                response.Content = new StringContent(responseBody, Encoding.UTF8, "application/json");
            }

            return Task.FromResult(response);
        }
    }
}
