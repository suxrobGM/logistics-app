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
using Logistics.Application.Tests.TestKit;
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
        var httpClient = new HttpClient(new NeverCalledHttpHandler()) { BaseAddress = new Uri("https://example") };
        var client = new GeotabClient(httpClient, NullLogger<GeotabClient>.Instance);
        var options = Options.Create(new EldOptions());
        sut = new GeotabEldService(client, tenantUow, options, NullLogger<GeotabEldService>.Instance);
    }

    [Fact]
    public async Task ProcessWebhook_ValidSignature_ParsesPayload()
    {
        var signature = WebhookTestKit.ComputeHmacHex(payload, secret);

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
        var result = await sut.ProcessWebhookAsync(payload, signature: null, webhookSecret: null);

        Assert.False(result.IsValid);
        Assert.NotNull(result.ErrorMessage);
    }

    [Fact]
    public async Task ProcessWebhook_MalformedJson_ReturnsInvalid()
    {
        const string payload = "{not json";
        var signature = WebhookTestKit.ComputeHmacHex(payload, secret);

        var result = await sut.ProcessWebhookAsync(payload, signature, secret);

        Assert.False(result.IsValid);
        Assert.NotNull(result.ErrorMessage);
    }
}
