using System.Security.Cryptography;
using System.Text;
using Logistics.Application.Abstractions.Payments;
using Logistics.Infrastructure.Payments.Stripe;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

namespace Logistics.Infrastructure.Tests.Payments.Stripe;

public class StripeWebhookParserTests
{
    private const string Secret = "whsec_test";

    private readonly StripeWebhookParser _sut = new(
        Options.Create(new StripeOptions { WebhookSecret = Secret }),
        NullLogger<StripeWebhookParser>.Instance);

    [Fact]
    public void Parse_BadSignature_ReturnsNull()
    {
        var body = EventJson("payment_intent.succeeded", """{ "id": "pi_1", "object": "payment_intent" }""");

        var parsed = _sut.Parse(body, Sign(body, "whsec_other"));

        Assert.Null(parsed);
    }

    [Fact]
    public void Parse_UnhandledType_ReturnsOtherWithoutPayload()
    {
        var body = EventJson("customer.created", """{ "id": "cus_1", "object": "customer" }""");

        var parsed = _sut.Parse(body, Sign(body, Secret));

        Assert.NotNull(parsed);
        Assert.Equal(StripeWebhookEventKind.Other, parsed.Kind);
        Assert.Equal("customer.created", parsed.Type);
        Assert.Null(parsed.Subscription);
        Assert.Null(parsed.PaymentIntent);
        Assert.Null(parsed.Account);
        Assert.Null(parsed.CheckoutSession);
    }

    [Fact]
    public void Parse_CheckoutSessionCompletedInJpy_KeepsZeroDecimalAmount()
    {
        var body = EventJson("checkout.session.completed", """
            {
              "id": "cs_1",
              "object": "checkout.session",
              "amount_total": 5000,
              "currency": "jpy",
              "payment_status": "paid",
              "payment_intent": "pi_1",
              "metadata": { "tenant_id": "t1", "invoice_id": "i1" }
            }
            """);

        var parsed = _sut.Parse(body, Sign(body, Secret));

        Assert.NotNull(parsed);
        Assert.Equal(StripeWebhookEventKind.CheckoutSessionCompleted, parsed.Kind);
        var session = Assert.IsType<StripeCheckoutSessionInfo>(parsed.CheckoutSession);
        Assert.Equal(5000m, session.AmountTotal);
        Assert.Equal("pi_1", session.PaymentIntentId);
        Assert.Equal("paid", session.PaymentStatus);
        Assert.Equal("i1", session.Metadata[StripeMetadataKeys.InvoiceId]);
    }

    [Fact]
    public void Parse_PaymentIntentFailed_MapsErrorMessage()
    {
        var body = EventJson("payment_intent.payment_failed", """
            {
              "id": "pi_1",
              "object": "payment_intent",
              "status": "requires_payment_method",
              "last_payment_error": { "message": "Card declined" },
              "metadata": { "tenant_id": "t1" }
            }
            """);

        var parsed = _sut.Parse(body, Sign(body, Secret));

        Assert.NotNull(parsed);
        Assert.Equal(StripeWebhookEventKind.PaymentIntentFailed, parsed.Kind);
        var paymentIntent = Assert.IsType<StripePaymentIntentInfo>(parsed.PaymentIntent);
        Assert.Equal("Card declined", paymentIntent.LastErrorMessage);
        Assert.Equal("t1", paymentIntent.Metadata[StripeMetadataKeys.TenantId]);
    }

    private static string EventJson(string type, string dataObject) => $$"""
        {
          "id": "evt_1",
          "object": "event",
          "type": "{{type}}",
          "data": { "object": {{dataObject}} }
        }
        """;

    /// <summary>Builds a header in Stripe's <c>t=...,v1=...</c> format: HMAC-SHA256 of "{timestamp}.{body}".</summary>
    private static string Sign(string body, string secret)
    {
        var timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        var hash = HMACSHA256.HashData(Encoding.UTF8.GetBytes(secret), Encoding.UTF8.GetBytes($"{timestamp}.{body}"));
        return $"t={timestamp},v1={Convert.ToHexStringLower(hash)}";
    }
}
