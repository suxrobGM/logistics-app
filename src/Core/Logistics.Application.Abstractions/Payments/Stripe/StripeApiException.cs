namespace Logistics.Application.Abstractions.Payments.Stripe;

/// <summary>Stripe rejected a request; lets callers handle it without referencing the Stripe SDK.</summary>
public sealed class StripeApiException(string message, string? code = null, Exception? innerException = null)
    : Exception(message, innerException)
{
    /// <summary>Stripe's error code, for example <c>resource_missing</c>.</summary>
    public string? Code { get; } = code;
}
