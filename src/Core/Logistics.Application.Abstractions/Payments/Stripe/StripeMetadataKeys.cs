namespace Logistics.Application.Abstractions.Payments.Stripe;

public static class StripeMetadataKeys
{
    public const string TenantId = "tenant_id";
    public const string PlanId = "plan_id";

    /// <summary>On a public checkout session: the invoice the payment is for.</summary>
    public const string InvoiceId = "invoice_id";

    /// <summary>On a public checkout session: the payment link it was opened from.</summary>
    public const string PaymentLinkToken = "payment_link_token";
}
