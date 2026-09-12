using System.Security.Cryptography;
using System.Text;
using Logistics.Application.Abstractions.LoadBoard;
using Logistics.Infrastructure.Integrations.LoadBoard;
using Logistics.Infrastructure.Integrations.LoadBoard.Providers.Dat;
using Logistics.Infrastructure.Integrations.LoadBoard.Providers.OneTwo3;
using Logistics.Infrastructure.Integrations.LoadBoard.Providers.Truckstop;
using Logistics.Shared.Models;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using NSubstitute;
using Xunit;

namespace Logistics.Application.Tests.LoadBoard;

/// <summary>
/// The <c>webhooks/loadboard/*</c> endpoints are anonymous, so the HMAC signature is the only
/// authenticity check. Every provider must reject a webhook it cannot verify.
/// </summary>
public class LoadBoardWebhookTests
{
    private const string secret = "wh-secret";

    public static TheoryData<string> Providers => ["dat", "truckstop", "123loadboard"];

    [Theory]
    [MemberData(nameof(Providers))]
    public async Task ProcessWebhook_NoSecretConfigured_RejectsPayload(string provider)
    {
        var sut = Create(provider);

        var result = await sut.ProcessWebhookAsync(PayloadFor(provider), signature: null, webhookSecret: null);

        Assert.False(result.IsValid);
        Assert.Equal("Invalid webhook signature", result.ErrorMessage);
        Assert.Equal(LoadBoardWebhookEventType.Unknown, result.EventType);
    }

    [Theory]
    [MemberData(nameof(Providers))]
    public async Task ProcessWebhook_SignaturePresentButNoSecretConfigured_RejectsPayload(string provider)
    {
        var sut = Create(provider);
        var payload = PayloadFor(provider);

        var result = await sut.ProcessWebhookAsync(payload, ComputeHex(payload, secret), webhookSecret: null);

        Assert.False(result.IsValid);
        Assert.Equal("Invalid webhook signature", result.ErrorMessage);
    }

    [Theory]
    [MemberData(nameof(Providers))]
    public async Task ProcessWebhook_InvalidSignature_RejectsBeforeParsing(string provider)
    {
        var sut = Create(provider);

        var result = await sut.ProcessWebhookAsync(PayloadFor(provider), "0000", secret);

        Assert.False(result.IsValid);
        Assert.Equal("Invalid webhook signature", result.ErrorMessage);
    }

    [Theory]
    [MemberData(nameof(Providers))]
    public async Task ProcessWebhook_ValidSignature_ParsesPayload(string provider)
    {
        var sut = Create(provider);
        var payload = PayloadFor(provider);

        var result = await sut.ProcessWebhookAsync(payload, ComputeHex(payload, secret), secret);

        Assert.True(result.IsValid);
        Assert.Equal(LoadBoardWebhookEventType.LoadPosted, result.EventType);
        Assert.Equal("L-1", result.ExternalListingId);
    }

    [Theory]
    [MemberData(nameof(Providers))]
    public async Task ProcessWebhook_MalformedJsonWithValidSignature_ReturnsInvalid(string provider)
    {
        const string payload = "{not json";
        var sut = Create(provider);

        var result = await sut.ProcessWebhookAsync(payload, ComputeHex(payload, secret), secret);

        Assert.False(result.IsValid);
        Assert.NotNull(result.ErrorMessage);
    }

    private static string PayloadFor(string provider) => provider switch
    {
        // Truckstop names the field "Event". The other two use "EventType".
        "truckstop" => """{"Event":"load_posted","LoadId":"L-1"}""",
        _ => """{"EventType":"load.posted","LoadId":"L-1"}"""
    };

    private static ILoadBoardProviderService Create(string provider)
    {
        var httpClient = new HttpClient(new NeverCalledHandler()) { BaseAddress = new Uri("https://example") };
        var httpClientFactory = Substitute.For<IHttpClientFactory>();
        var options = Options.Create(new LoadBoardOptions());

        return provider switch
        {
            "dat" => new DatLoadBoardService(
                httpClient, httpClientFactory, options, NullLogger<DatLoadBoardService>.Instance),
            "truckstop" => new TruckstopLoadBoardService(
                httpClient, httpClientFactory, options, NullLogger<TruckstopLoadBoardService>.Instance),
            "123loadboard" => new OneTwo3LoadBoardService(
                httpClient, httpClientFactory, options,
                Substitute.For<IOneTwo3SearchRateLimiter>(),
                NullLogger<OneTwo3LoadBoardService>.Instance),
            _ => throw new ArgumentOutOfRangeException(nameof(provider), provider, "Unknown load board provider")
        };
    }

    private static string ComputeHex(string payload, string secret)
    {
        var hash = HMACSHA256.HashData(Encoding.UTF8.GetBytes(secret), Encoding.UTF8.GetBytes(payload));
        return Convert.ToHexStringLower(hash);
    }

    private sealed class NeverCalledHandler : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken ct)
        {
            throw new InvalidOperationException("Webhook tests must not perform HTTP calls.");
        }
    }
}
