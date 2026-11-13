using System.Net;
using Logistics.Infrastructure.Integrations.LoadBoard.Credit;
using Logistics.Infrastructure.Tests.TestKit;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

namespace Logistics.Infrastructure.Tests.Integrations.LoadBoard;

public class FmcsaClientTests
{
    private static FmcsaClient CreateClient(string? responseBody, HttpStatusCode statusCode = HttpStatusCode.OK, string? webKey = "test-key")
    {
        var handler = responseBody is null
            ? new StubHttpMessageHandler(_ => new HttpResponseMessage(statusCode))
            : StubHttpMessageHandler.Json(responseBody, statusCode);
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
        var handler = StubHttpMessageHandler.Json("{}");
        var client = new FmcsaClient(
            new HttpClient(handler),
            Options.Create(new FmcsaOptions { WebKey = null }),
            NullLogger<FmcsaClient>.Instance);

        var result = await client.GetAuthorityActiveAsync("123456");

        Assert.Null(result);
        Assert.Empty(handler.Requests);
    }
}
