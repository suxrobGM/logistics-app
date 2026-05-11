using System.Net;

namespace Logistics.Infrastructure.Vin.Tests;

internal sealed class StubHttpMessageHandler(Func<HttpRequestMessage, HttpResponseMessage> handler) : HttpMessageHandler
{
    private readonly Func<HttpRequestMessage, HttpResponseMessage> handler = handler;

    public static StubHttpMessageHandler Json(string body, HttpStatusCode status = HttpStatusCode.OK) =>
        new(_ => new HttpResponseMessage(status)
        {
            Content = new StringContent(body, System.Text.Encoding.UTF8, "application/json")
        });

    public static StubHttpMessageHandler Throws(Exception ex) =>
        new(_ => throw ex);

    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        return Task.FromResult(handler(request));
    }
}
