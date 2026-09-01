using System.Security.Cryptography;
using System.Text;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Domain.Primitives.Enums;
using Logistics.Domain.Primitives.ValueObjects;
using Logistics.Infrastructure.Integrations.Eld;
using Logistics.Infrastructure.Integrations.Eld.Providers.Geotab;
using Logistics.Shared.Models;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using NSubstitute;
using Xunit;

namespace Logistics.Application.Tests.Eld;

public class GeotabWebhookTests
{
    private const string secret = "wh-secret";
    private const string payload = """{"eventType":"violationCreated","driverId":"d-42","vehicleId":"v-7"}""";

    private readonly ITenantUnitOfWork tenantUow = Substitute.For<ITenantUnitOfWork>();
    private readonly GeotabEldService sut;

    public GeotabWebhookTests()
    {
        var httpClient = new HttpClient(new NeverCalledHandler()) { BaseAddress = new Uri("https://example") };
        var client = new GeotabClient(httpClient, NullLogger<GeotabClient>.Instance);
        var options = Options.Create(new EldOptions());
        sut = new GeotabEldService(client, tenantUow, options, NullLogger<GeotabEldService>.Instance);
    }

    [Fact]
    public async Task ProcessWebhook_ValidSignature_ParsesPayload()
    {
        var signature = ComputeHex(payload, secret);

        var result = await sut.ProcessWebhookAsync(payload, signature, secret);

        Assert.True(result.IsValid);
        Assert.Equal(EldWebhookEventType.ViolationCreated, result.EventType);
        Assert.Equal("d-42", result.ExternalDriverId);
        Assert.Equal("v-7", result.ExternalVehicleId);
    }

    [Fact]
    public async Task ProcessWebhook_InvalidSignature_RejectsBeforeParsing()
    {
        var result = await sut.ProcessWebhookAsync(payload, "0000", secret);

        Assert.False(result.IsValid);
        Assert.Equal("Invalid webhook signature", result.ErrorMessage);
    }

    [Fact]
    public async Task ProcessWebhook_NoSecretConfigured_RejectsPayload()
    {
        // With no secret configured an anonymous webhook has no authenticity check at all, so it
        // is rejected rather than processed unverified.
        var result = await sut.ProcessWebhookAsync(payload, signature: null, webhookSecret: null);

        Assert.False(result.IsValid);
        Assert.NotNull(result.ErrorMessage);
    }

    [Fact]
    public async Task ProcessWebhook_MalformedJson_ReturnsInvalid()
    {
        // Sign the malformed body with the real secret so it passes the signature gate and the
        // JSON-parse path this test is named for is actually exercised (a null signature would be
        // rejected fail-closed before parsing ever runs).
        const string payload = "{not json";
        var signature = ComputeHex(payload, secret);

        var result = await sut.ProcessWebhookAsync(payload, signature, secret);

        Assert.False(result.IsValid);
        Assert.NotNull(result.ErrorMessage);
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
