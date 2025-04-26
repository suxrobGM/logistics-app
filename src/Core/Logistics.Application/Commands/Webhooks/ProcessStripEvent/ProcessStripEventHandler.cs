using Logistics.Application.Services;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Shared.Consts;
using Logistics.Shared.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Stripe;
using PaymentMethod = Logistics.Domain.Entities.PaymentMethod;
using StripeInvoice = Stripe.Invoice;
using StripePaymentMethod = Stripe.PaymentMethod;

namespace Logistics.Application.Commands;

internal sealed class ProcessStripEventHandler : RequestHandler<ProcessStripEventCommand, Result>
{
    private readonly ITenantUnityOfWork _tenantUow;
    private readonly string _stripeWebhookSecret;
    private readonly ILogger<ProcessStripEventHandler> _logger;

    public ProcessStripEventHandler(
        ITenantUnityOfWork tenantUow,
        IOptions<StripeOptions> stripeOptions,
        ILogger<ProcessStripEventHandler> logger)
    {
        _tenantUow = tenantUow;
        _logger = logger;
        _stripeWebhookSecret = stripeOptions.Value.WebhookSecret ?? throw new ArgumentNullException(nameof(stripeOptions));
    }

    protected override async Task<Result> HandleValidated(
        ProcessStripEventCommand req, CancellationToken cancellationToken)
    {
        try
        {
            var stripeEvent = EventUtility.ConstructEvent(
                req.RequestBodyJson, 
                req.StripeSignature, 
                _stripeWebhookSecret, 
                throwOnApiVersionMismatch: false);
            
            _logger.LogInformation("Received Stripe event: {Type}", stripeEvent.Type);

            switch (stripeEvent.Type)
            {
                case EventTypes.InvoicePaid:
                    var invoice = stripeEvent.Data.Object as StripeInvoice;
                    return await HandleInvoicePaid(invoice!);
                case EventTypes.PaymentMethodAttached:
                    var paymentMethod = stripeEvent.Data.Object as StripePaymentMethod;
                    return await HandlePaymentAttached(paymentMethod!);
            }
        
            return Result.Succeed();
        }
        catch (StripeException ex)
        {
            _logger.LogError(ex, "Stripe error: {Message}", ex.Message);
            return Result.Fail(ex.Message);
        }
    }

    private async Task<Result> HandleInvoicePaid(StripeInvoice stripeInvoice)
    {
        if (!stripeInvoice.Metadata.TryGetValue(StripeMetadataKeys.TenantId, out var tenantId))
        {
            return Result.Fail($"{StripeMetadataKeys.TenantId} not found in invoice metadata.");
        }
        
        _tenantUow.SetCurrentTenantById(tenantId);
        
        var payment = new Payment
        {
            Amount = stripeInvoice.AmountPaid / 100m, // Convert from cents
            Status = PaymentStatus.Paid,
            PaymentDate = DateTime.UtcNow,
            StripeInvoiceId = stripeInvoice.Id,
            StripePaymentIntentId = stripeInvoice.PaymentIntent.Id,
            BillingAddress = stripeInvoice.CustomerAddress.ToAddressEntity(),
        };
        
        await _tenantUow.Repository<Payment>().AddAsync(payment);
        await _tenantUow.SaveChangesAsync();
        
        _logger.LogInformation("Added payment for tenant {TenantId} with amount {Amount}, invoice ID {StripeInvoiceId}",
            tenantId, payment.Amount, stripeInvoice.Id);
        return Result.Succeed();
    }

    private async Task<Result> HandlePaymentAttached(StripePaymentMethod stripePaymentMethod)
    {
        if (!stripePaymentMethod.Metadata.TryGetValue(StripeMetadataKeys.TenantId, out var tenantId))
        {
            return Result.Fail($"{StripeMetadataKeys.TenantId} not found in payment method metadata.");
        }
        
        _tenantUow.SetCurrentTenantById(tenantId);
        var paymentMethod = stripePaymentMethod.ToPaymentMethodEntity();
        await _tenantUow.Repository<PaymentMethod>().AddAsync(paymentMethod);
        await _tenantUow.SaveChangesAsync();
        
        _logger.LogInformation("Attached payment method {PaymentMethodType} {StripePaymentMethodId} to tenant {TenantId}",
            paymentMethod.Type, paymentMethod.StripePaymentMethodId, tenantId);
        return Result.Succeed();
    }
}
