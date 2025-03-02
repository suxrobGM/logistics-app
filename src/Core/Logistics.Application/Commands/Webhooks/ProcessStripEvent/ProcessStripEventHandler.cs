using Logistics.Application.Services;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Shared.Consts;
using Logistics.Shared.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Stripe;
using Subscription = Logistics.Domain.Entities.Subscription;
using StripeInvoice = Stripe.Invoice;

namespace Logistics.Application.Commands;

internal sealed class ProcessStripEventHandler : RequestHandler<ProcessStripEventCommand, Result>
{
    private readonly IMasterUnityOfWork _masterUow;
    private readonly string _stripeWebhookSecret;
    private readonly ILogger<ProcessStripEventHandler> _logger;

    public ProcessStripEventHandler(
        IMasterUnityOfWork masterUow,
        IOptions<StripeOptions> stripeOptions,
        ILogger<ProcessStripEventHandler> logger)
    {
        _masterUow = masterUow;
        _logger = logger;
        _stripeWebhookSecret = stripeOptions.Value.WebhookSecret ?? throw new ArgumentNullException(nameof(stripeOptions));
    }

    protected override async Task<Result> HandleValidated(
        ProcessStripEventCommand req, CancellationToken cancellationToken)
    {
        try
        {
            var stripeEvent = EventUtility.ConstructEvent(req.RequestBodyJson, req.StripeSignature, _stripeWebhookSecret);
            _logger.LogInformation("Received Stripe event: {Type}", stripeEvent.Type);

            switch (stripeEvent.Type)
            {
                case EventTypes.InvoicePaid:
                    var invoice = stripeEvent.Data.Object as StripeInvoice;
                    return await HandleInvoicePaid(invoice!);
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
        var subsRepository = _masterUow.Repository<Subscription>();
        var subsPaymentRepository = _masterUow.Repository<SubscriptionPayment>();
        var subscription = await subsRepository.GetAsync(i => i.StripeSubscriptionId == stripeInvoice.Subscription.Id);

        if (subscription is null)
        {
            return Result.Fail($"Could not find a subscription with Stripe ID '{stripeInvoice.Subscription.Id}'");
        }
        
        var payment = new SubscriptionPayment
        {
            Amount = stripeInvoice.AmountPaid / 100m, // Convert from cents
            Status = PaymentStatus.Paid,
            PaymentDate = DateTime.UtcNow,
            StripeInvoiceId = stripeInvoice.Id,
            Subscription = subscription,
            SubscriptionId = subscription.Id
        };
        
        await subsPaymentRepository.AddAsync(payment);
        await _masterUow.SaveChangesAsync();
        return Result.Succeed();
    }
}
